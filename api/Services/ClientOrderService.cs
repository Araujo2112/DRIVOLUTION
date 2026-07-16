using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável pela lógica de negócio das encomendas dos clientes
public class ClientOrderService : IClientOrderService
{
    // Repository principal das encomendas dos clientes
    private readonly IClientOrderRepository _repo;

    // Repository usado para criar e atualizar ordens de fabrico
    private readonly IManufacturingOrderRepository _manufacturingOrderRepo;

    // Repository usado para criar os produtos físicos
    private readonly IProductRepository _productRepo;

    // Repository usado para validar o modelo de carro
    private readonly ICarModelRepository _carModelRepo;

    // Repository usado para guardar as configurações escolhidas para cada produto
    private readonly IProductConfigRepository _productConfigRepo;

    // Repository usado para obter as categorias de configuração do modelo
    private readonly IConfigRepository _configRepo;

    // Repository usado para consultar as opções de cada configuração
    private readonly IConfigOptionRepository _configOptionRepo;

    // O ASP.NET injeta automaticamente todos os repositories necessários
    public ClientOrderService(
        IClientOrderRepository repo,
        IManufacturingOrderRepository manufacturingOrderRepo,
        IProductRepository productRepo,
        ICarModelRepository carModelRepo,
        IProductConfigRepository productConfigRepo,
        IConfigRepository configRepo,
        IConfigOptionRepository configOptionRepo)
    {
        _repo = repo;
        _manufacturingOrderRepo = manufacturingOrderRepo;
        _productRepo = productRepo;
        _carModelRepo = carModelRepo;
        _productConfigRepo = productConfigRepo;
        _configRepo = configRepo;
        _configOptionRepo = configOptionRepo;
    }

    // Deriva o estado da encomenda a partir das suas ManufacturingOrders.
    // Regras (por prioridade):
    //   - Sem MOs         → "pending"
    //   - Todas Concluídas → "completed"
    //   - Todas Canceladas → "cancelled"
    //   - Alguma Em Progresso (e não todas concluídas/canceladas) → "in_progress"
    //   - Caso contrário  → "pending"
    private static string DeriveStatus(ICollection<ManufacturingOrderModel> mos)
    {
        // Se ainda não existem ordens de fabrico, a encomenda está pendente
        if (!mos.Any()) return OrderStatus.Pending;

        // Se todas as ordens de fabrico terminaram, a encomenda está concluída
        if (mos.All(m => m.Status == OrderStatus.Completed))
            return OrderStatus.Completed;

        // Se todas foram canceladas, a encomenda está cancelada
        if (mos.All(m => m.Status == OrderStatus.Cancelled))
            return OrderStatus.Cancelled;

        // Se pelo menos uma está em produção, considera-se em progresso
        if (mos.Any(m => m.Status == OrderStatus.InProgress))
            return OrderStatus.InProgress;

        // Nos restantes casos, continua pendente
        return OrderStatus.Pending;
    }

    // Devolve uma lista paginada de encomendas, aplicando filtros
    public async Task<PagedResultDTO<ClientOrderDTO>> GetPaged(
        int page,
        int pageSize,
        string? search,
        string? status,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        // Obtém todas as encomendas, incluindo utilizador e ordens de fabrico
        var all = await _repo.GetAll();

        // Calcular status em memória e aplicar filtros
        var filtered = all
            // Para cada encomenda, calcula primeiro o respetivo estado
            .Select(c => new
            {
                Model = c,
                Status = DeriveStatus(c.ManufacturingOrders)
            })
            .Where(x =>
            {
                // Filtro de pesquisa pelo número da encomenda ou nome do cliente
                if (!string.IsNullOrWhiteSpace(search))
                {
                    var q = search.Trim().ToLower();

                    if (!x.Model.OrderNumber.ToLower().Contains(q) &&
                        !(x.Model.AppUser?.Name ?? "").ToLower().Contains(q))
                        return false;
                }

                // Filtro pelo estado calculado
                if (!string.IsNullOrWhiteSpace(status) &&
                    x.Status != status)
                    return false;

                // Filtro pela data inicial
                if (dateFrom.HasValue &&
                    x.Model.OrderDate.Date < dateFrom.Value.Date)
                    return false;

                // Filtro pela data final
                if (dateTo.HasValue &&
                    x.Model.OrderDate.Date > dateTo.Value.Date)
                    return false;

                return true;
            })
            .ToList();

        // Número total de resultados depois dos filtros
        var total = filtered.Count;

        // Aplica a paginação e converte para DTO
        var data = filtered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ClientOrderDTO(
                x.Model.Id,
                x.Model.OrderNumber,
                x.Model.OrderDate,
                x.Model.AppUserId,
                x.Model.AppUser?.Name ?? "",
                x.Model.Quantity,
                x.Status
            ));

        // Devolve os dados juntamente com a informação da paginação
        return new PagedResultDTO<ClientOrderDTO>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Procura uma encomenda pelo seu ID
    public async Task<ClientOrderDTO?> GetById(int id)
    {
        var item = await _repo.GetById(id);

        // Se não existir, devolve null
        if (item == null) return null;

        // Converte a encomenda para DTO e calcula o seu estado
        return new ClientOrderDTO(
            item.Id,
            item.OrderNumber,
            item.OrderDate,
            item.AppUserId,
            item.AppUser?.Name ?? "",
            item.Quantity,
            DeriveStatus(item.ManufacturingOrders)
        );
    }

    // Determina quais as opções finais que devem ser aplicadas
    // a uma categoria de configuração
    private async Task<List<int>> ResolveOptionIds(
        ConfigModel configCategory,
        List<ConfigSelectionDTO>? selections)
    {
        // Lista onde serão guardados os IDs válidos encontrados
        var matchedIds = new List<int>();

        // Verifica se o cliente enviou seleções
        if (selections != null && selections.Any())
        {
            foreach (var selection in selections)
            {
                // Procura a opção selecionada
                var option = await _configOptionRepo
                    .GetById(selection.ConfigOptionId);

                // Confirma que a opção pertence à categoria atual
                if (option != null &&
                    option.ConfigId == configCategory.Id)
                {
                    matchedIds.Add(option.Id);

                    // Numa categoria de seleção única, só aceita uma opção
                    if (!configCategory.AllowMultiple)
                        break; // single-select: ignora seleções extra da mesma categoria
                }
            }
        }

        // Se foram encontradas seleções válidas, utiliza-as
        if (matchedIds.Any())
            return matchedIds;

        // Fallback: nenhuma seleção do cliente para esta categoria
        if (configCategory.AllowMultiple)
        {
            // Para categorias de escolha múltipla,
            // devolve todas as opções marcadas como padrão
            var defaults =
                await _configOptionRepo
                    .GetDefaultsByConfigId(configCategory.Id);

            return defaults
                .Select(d => d.Id)
                .ToList();
        }
        else
        {
            // Para categorias de escolha única,
            // devolve apenas a opção padrão
            var defaultOption =
                await _configOptionRepo
                    .GetDefaultByConfigId(configCategory.Id);

            return defaultOption != null
                ? new List<int> { defaultOption.Id }
                : new List<int>();
        }
    }

    // Cria uma nova encomenda de cliente
    public async Task<CreateClientOrderResultDTO> Create(CreateClientOrderDTO dto)
    {
        // 1. Validar existência do modelo de carro
        var carModel = await _carModelRepo.GetById(dto.ModelId);

        if (carModel == null)
            throw new KeyNotFoundException(
                "Modelo de carro não encontrado."
            );

        // 2. Criar o registo da Encomenda do Cliente
        var clientOrder = new ClientOrderModel
        {
            OrderNumber = dto.OrderNumber,
            OrderDate = dto.OrderDate,
            AppUserId = dto.AppUserId,
            Quantity = dto.Quantity
        };

        // Guarda a encomenda
        var createdOrder = await _repo.Create(clientOrder);

        // 3. Obter todas as categorias de configuração definidas para este modelo
        var allConfigsForModel =
            await _configRepo.GetByModelId(dto.ModelId);

        // Lista usada para devolver um resumo dos produtos criados
        var summaryItems = new List<ProductSummaryDTO>();

        // 4. Gerar os itens físicos (Products) e Ordens de Fabrico (MOs)
        // É criada uma MO e um produto por cada unidade encomendada
        for (int i = 1; i <= dto.Quantity; i++)
        {
            // Cria uma ordem de fabrico individual
            var mo = await _manufacturingOrderRepo.Create(
                new ManufacturingOrderModel
                {
                    ClientOrderId = createdOrder.Id,

                    // Exemplo: ORDER-001-MO-001
                    ManufacturingOrderNumber =
                        $"{dto.OrderNumber}-MO-{i:D3}",

                    StartDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending
                }
            );

            // Cria o produto físico associado à ordem de fabrico
            var product = await _productRepo.Create(
                new ProductModel
                {
                    ManufacturingOrderId = mo.Id,
                    ModelId = dto.ModelId,

                    // Gera um VIN único com parte de um GUID
                    SerialNumber =
                        $"VIN-{dto.OrderNumber}-" +
                        $"{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",

                    LotNumber = $"LOT-{dto.OrderNumber}"
                }
            );

            // 5. Processar as Configurações para cada Produto
            foreach (var configCategory in allConfigsForModel)
            {
                // Determina as opções escolhidas ou os valores padrão
                var finalOptionIds =
                    await ResolveOptionIds(
                        configCategory,
                        dto.Configs
                    );

                // Associa cada opção final ao produto
                foreach (var optionId in finalOptionIds)
                {
                    await _productConfigRepo.Create(
                        new ProductConfigModel
                        {
                            ProductId = product.Id,
                            ConfigOptionId = optionId
                        }
                    );
                }
            }

            // Adiciona o produto criado ao resumo final
            summaryItems.Add(
                new ProductSummaryDTO(
                    product.Id,
                    product.SerialNumber,
                    mo.ManufacturingOrderNumber
                )
            );
        }

        // Devolve o resultado com a encomenda e os produtos criados
        return new CreateClientOrderResultDTO(
            createdOrder.Id,
            createdOrder.AppUser?.Name ?? "",
            createdOrder.Quantity,
            summaryItems
        );
    }

    // Atualiza os dados básicos de uma encomenda
    public async Task<bool> Update(int id, UpdateClientOrderDTO dto)
    {
        // Procura a encomenda
        var entity = await _repo.GetById(id);

        // Se não existir, informa que a atualização falhou
        if (entity == null) return false;

        // Atualiza apenas os campos enviados
        if (dto.OrderNumber != null)
            entity.OrderNumber = dto.OrderNumber;

        if (dto.OrderDate != null)
            entity.OrderDate = dto.OrderDate.Value;

        if (dto.AppUserId != null)
            entity.AppUserId = dto.AppUserId.Value;

        if (dto.Quantity != null)
            entity.Quantity = dto.Quantity.Value;

        // Guarda as alterações
        await _repo.Update(entity);

        return true;
    }

    // Elimina uma encomenda
    public async Task<bool> Delete(int id)
    {
        // Verifica se existe
        if (!await _repo.Exists(id))
            return false;

        // Elimina o registo
        await _repo.Delete(id);

        return true;
    }

    // Cancela uma encomenda: marca todas as ManufacturingOrders ainda não concluídas
    // como "cancelled". Não elimina nenhum registo.
    // Retorna false se a encomenda não existir ou já estiver totalmente concluída/cancelada.
    public async Task<bool> Cancel(int id)
    {
        // Procura a encomenda, já com as ManufacturingOrders carregadas
        var entity = await _repo.GetById(id); // já inclui ManufacturingOrders via Include

        if (entity == null)
            return false;

        // Seleciona apenas as ordens de fabrico
        // que ainda podem ser canceladas
        var cancellable = entity.ManufacturingOrders
            .Where(m =>
                m.Status != OrderStatus.Completed &&
                m.Status != OrderStatus.Cancelled)
            .ToList();

        // Se não existir nenhuma MO cancelável, devolve false
        if (!cancellable.Any())
            return false;

        // Cancela cada ordem de fabrico
        foreach (var mo in cancellable)
        {
            mo.Status = OrderStatus.Cancelled;
            mo.EndDate = DateTime.UtcNow;

            await _manufacturingOrderRepo.Update(mo);
        }

        return true;
    }
}