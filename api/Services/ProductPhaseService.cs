using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Implementa o definido em IProductPhaseService
public class ProductPhaseService : IProductPhaseService
{
    // Repository responsável pelas fases dos produtos
    private readonly IProductPhaseRepository _repo;

    // Repository usado para consultar os produtos
    private readonly IProductRepository _productRepo;

    // Repository usado para consultar e atualizar ordens de fabrico
    private readonly IManufacturingOrderRepository _moRepo;

    // Service usado para criar notificações para o cliente
    private readonly INotificationService _notificationService;

    // Repository usado para verificar os controlos de qualidade
    private readonly IQualityCheckRepository _qualityRepo;

    // O ASP.NET injeta automaticamente todos os repositorios e services necessários
    public ProductPhaseService(
        IProductPhaseRepository repo,
        IProductRepository productRepo,
        IManufacturingOrderRepository moRepo,
        INotificationService notificationService,
        IQualityCheckRepository qualityRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
        _moRepo = moRepo;
        _notificationService = notificationService;
        _qualityRepo = qualityRepo;
    }

    // Devolve todas as fases associadas a um produto
    public async Task<IEnumerable<ProductPhaseDTO>> GetByProduct(int productId)
    {
        // Vai buscar as fases à base de dados
        var items = await _repo.GetByProduct(productId);

        // Converte cada ProductPhaseModel para ProductPhaseDTO
        return items.Select(ToDTO);
    }

    // Devolve a fase atual, ou seja, a fase que ainda não terminou
    public async Task<ProductPhaseDTO?> GetCurrent(int productId)
    {
        // Procura a fase atual do produto
        var item = await _repo.GetCurrentByProduct(productId);

        // Se não existir devolve null; se existir converte para DTO
        return item == null ? null : ToDTO(item);
    }

    // Cria uma nova fase para o produto
    public async Task<ProductPhaseDTO> Create(CreateProductPhaseDTO dto)
    {
        // Procura todas as fases do produto que ainda estão abertas
        var openPhases = await _repo.GetAllOpenByProduct(dto.ProductId);

        // Verifica se o produto pode avançar para a nova fase
        foreach (var open in openPhases)
        {
            // Impede que o produto entre novamente na fase onde já está
            if (open.ManufacturingPhaseId == dto.ManufacturingPhaseId)
            {
                throw new InvalidOperationException(
                    "Este produto já está nesta fase."
                );
            }

            // Verifica se a fase aberta passou no controlo de qualidade
            var hasPassedQuality =
                await _qualityRepo.HasPassedForProductPhaseAsync(
                    dto.ProductId,
                    open.ManufacturingPhaseId
                );

            // Se não passou, o produto não pode avançar
            if (!hasPassedQuality)
            {
                throw new InvalidOperationException(
                    $"Produto bloqueado: a fase anterior " +
                    $"({open.ManufacturingPhase?.Name ?? open.ManufacturingPhaseId.ToString()}) " +
                    $"não tem QualityCheck com estado 'passed'."
                );
            }
        }

        // Como o produto pode avançar, fecha todas as fases que ainda estavam abertas
        foreach (var open in openPhases)
        {
            // Guarda a data e hora em que a fase terminou
            open.DatetimeEnd = DateTime.UtcNow;

            // Atualiza a fase na base de dados
            await _repo.Update(open);
        }

        // Cria a entidade da nova fase
        var entity = new ProductPhaseModel
        {
            ProductId = dto.ProductId,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
            WorkstationId = dto.WorkstationId,
            Notes = dto.Notes,

            // A fase começa neste momento
            DatetimeIni = DateTime.UtcNow
        };

        // Guarda a nova fase na base de dados
        var created = await _repo.Create(entity);

        // Atualiza o estado da ordem de fabrico associada ao produto
        await UpdateMoStatus(dto.ProductId);

        // Converte a entidade criada para DTO
        return ToDTO(created);
    }

    // Fecha manualmente uma fase
    public async Task Close(int id, CloseProductPhaseDTO dto)
    {
        // Procura a fase pelo ID
        var entity = await _repo.GetById(id);

        // Se a fase não existir, lança um erro
        if (entity == null)
            throw new KeyNotFoundException("Fase não encontrada.");

        // Define a data e hora de fim da fase
        entity.DatetimeEnd = DateTime.UtcNow;

        // Guarda o resultado recebido
        entity.Result = dto.Result;

        // Associa o controlo de qualidade, se existir
        entity.QualityId = dto.QualityId;

        // Atualiza a fase na base de dados
        await _repo.Update(entity);

        // Atualiza o estado da ordem de fabrico
        await UpdateMoStatus(entity.ProductId);
    }

    // Atualiza automaticamente o estado da ordem de fabrico
    private async Task UpdateMoStatus(int productId)
    {
        // Procura o produto
        var product = await _productRepo.GetById(productId);

        // Se não existir, termina o método
        if (product == null) return;

        // Procura a ordem de fabrico com os produtos e fases relacionados
        var mo = await _moRepo.GetByIdWithDetails(
            product.ManufacturingOrderId
        );

        // Se a ordem não existir, termina o método
        if (mo == null) return;

        // Não altera ordens que já terminaram ou foram canceladas
        if (mo.Status == OrderStatus.Completed ||
            mo.Status == OrderStatus.Cancelled)
        {
            return;
        }

        // Guarda se a ordem estava pendente antes da alteração
        var wasPending = mo.Status == OrderStatus.Pending;

        // Converte os produtos da ordem para uma lista
        var allProducts = mo.Products.ToList();

        // Verifica se existe pelo menos um produto com uma fase aberta
        bool anyInProgress = allProducts.Any(product =>
            product.ProductPhases.Any(phase =>
                phase.DatetimeEnd == null
            )
        );

        // Verifica se todos os produtos têm fases
        // e se todas essas fases estão fechadas
        bool allCompleted = allProducts.All(product =>
            product.ProductPhases.Any() &&
            product.ProductPhases.All(phase =>
                phase.DatetimeEnd != null
            )
        );

        // Se todos os produtos terminaram, conclui a ordem
        if (allCompleted)
        {
            mo.Status = OrderStatus.Completed;
            mo.EndDate = DateTime.UtcNow;
        }
        // Se existir alguma fase aberta, coloca a ordem em produção
        else if (anyInProgress)
        {
            mo.Status = OrderStatus.InProgress;
        }

        // Guarda o novo estado da ordem
        await _moRepo.Update(mo);

        // Obtém o ID do cliente associado à encomenda
        var appUserId = mo.ClientOrder?.AppUserId;

        // Se não existir cliente associado, não envia notificações
        if (appUserId == null) return;

        // Se a ordem passou de pendente para em produção,
        // envia uma notificação ao cliente
        if (wasPending &&
            mo.Status == OrderStatus.InProgress)
        {
            await _notificationService.CreateAsync(
                appUserId.Value,
                "order_started",
                $"A tua encomenda {mo.ClientOrder!.OrderNumber} foi iniciada.",
                clientOrderId: mo.ClientOrderId
            );
        }

        // Procura o produto que originou esta atualização
        var thisProduct = allProducts.FirstOrDefault(
            product => product.Id == productId
        );

        // Se todas as fases desse produto estiverem fechadas,
        // informa o cliente de que o carro foi concluído
        if (thisProduct != null &&
            thisProduct.ProductPhases.Any() &&
            thisProduct.ProductPhases.All(
                phase => phase.DatetimeEnd != null
            ))
        {
            await _notificationService.CreateAsync(
                appUserId.Value,
                "car_completed",
                $"O carro {thisProduct.SerialNumber ?? "#" + thisProduct.Id} " +
                $"da encomenda {mo.ClientOrder!.OrderNumber} foi concluído.",
                clientOrderId: mo.ClientOrderId,
                productId: thisProduct.Id
            );
        }

        // Se esta ordem de fabrico ficou concluída
        if (mo.Status == OrderStatus.Completed)
        {
            // Procura todas as ordens de fabrico da mesma encomenda
            var siblingMos =
                await _moRepo.GetByClientOrderId(mo.ClientOrderId);

            // Verifica se todas as ordens da encomenda estão concluídas
            if (siblingMos.All(order =>
                order.Status == OrderStatus.Completed))
            {
                // Verifica se a notificação já foi enviada
                var already =
                    await _notificationService.ExistsAsync(
                        "order_completed",
                        mo.ClientOrderId
                    );

                // Só envia se ainda não existir
                if (!already)
                {
                    await _notificationService.CreateAsync(
                        appUserId.Value,
                        "order_completed",
                        $"A tua encomenda " +
                        $"{mo.ClientOrder!.OrderNumber} foi concluída.",
                        clientOrderId: mo.ClientOrderId
                    );
                }
            }
        }
    }

    // Converte um ProductPhaseModel para ProductPhaseDTO
    private static ProductPhaseDTO ToDTO(ProductPhaseModel pp) =>
        new(
            pp.Id,
            pp.ProductId,
            pp.ManufacturingPhaseId,

            // Se o nome da fase não existir, utiliza uma string vazia
            pp.ManufacturingPhase?.Name ?? "",

            pp.WorkstationId,
            pp.Notes,
            pp.Result,
            pp.DatetimeIni,
            pp.DatetimeEnd,
            pp.QualityId
        );
}