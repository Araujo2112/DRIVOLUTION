using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ClientOrderService : IClientOrderService
{
    private readonly IClientOrderRepository _repo;
    private readonly IManufacturingOrderRepository _manufacturingOrderRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICarModelRepository _carModelRepo;
    private readonly IProductConfigRepository _productConfigRepo;
    private readonly IConfigRepository _configRepo;
    private readonly IConfigOptionRepository _configOptionRepo;

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

    public async Task<IEnumerable<ClientOrderDTO>> GetAll()
    {
        var items = await _repo.GetAll();
        return items.Select(c => new ClientOrderDTO(c.Id, c.OrderNumber, c.OrderDate, c.CustomerName, c.Quantity));
    }

    public async Task<ClientOrderDTO?> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return null;
        return new ClientOrderDTO(item.Id, item.OrderNumber, item.OrderDate, item.CustomerName, item.Quantity);
    }

    private async Task<List<int>> ResolveOptionIds(ConfigModel configCategory, List<ConfigSelectionDTO>? selections)
    {
        var matchedIds = new List<int>();

        if (selections != null && selections.Any())
        {
            foreach (var selection in selections)
            {
                var option = await _configOptionRepo.GetById(selection.ConfigOptionId);
                if (option != null && option.ConfigId == configCategory.Id)
                {
                    matchedIds.Add(option.Id);
                    if (!configCategory.AllowMultiple)
                        break; // single-select: ignora seleções extra da mesma categoria
                }
            }
        }

        if (matchedIds.Any())
            return matchedIds;

        // Fallback: nenhuma seleção do cliente para esta categoria
        if (configCategory.AllowMultiple)
        {
            var defaults = await _configOptionRepo.GetDefaultsByConfigId(configCategory.Id);
            return defaults.Select(d => d.Id).ToList();
        }
        else
        {
            var defaultOption = await _configOptionRepo.GetDefaultByConfigId(configCategory.Id);
            return defaultOption != null ? new List<int> { defaultOption.Id } : new List<int>();
        }
    }

    public async Task<CreateClientOrderResultDTO> Create(CreateClientOrderDTO dto)
    {
        // 1. Validar existência do modelo de carro
        var carModel = await _carModelRepo.GetById(dto.ModelId);
        if (carModel == null)
            throw new KeyNotFoundException("Modelo de carro não encontrado.");

        // 2. Criar o registo da Encomenda do Cliente
        var clientOrder = new ClientOrderModel
        {
            OrderNumber = dto.OrderNumber,
            OrderDate = dto.OrderDate,
            CustomerName = dto.CustomerName,
            Quantity = dto.Quantity
        };
        var createdOrder = await _repo.Create(clientOrder);

        // 3. Obter todas as categorias de configuração definidas para este modelo
        var allConfigsForModel = await _configRepo.GetByModelId(dto.ModelId);
        var summaryItems = new List<ProductSummaryDTO>();

        // 4. Gerar os itens físicos (Products) e Ordens de Fabrico (MOs)
        for (int i = 1; i <= dto.Quantity; i++)
        {
            var mo = await _manufacturingOrderRepo.Create(new ManufacturingOrderModel
            {
                ClientOrderId = createdOrder.Id,
                ManufacturingOrderNumber = $"{dto.OrderNumber}-MO-{i:D3}",
                StartDate = DateTime.UtcNow,
                Status = OrderStatus.Pending
            });

            var product = await _productRepo.Create(new ProductModel
            {
                ManufacturingOrderId = mo.Id,
                ModelId = dto.ModelId,
                SerialNumber = $"VIN-{dto.OrderNumber}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                LotNumber = $"LOT-{dto.OrderNumber}"
            });

            // 5. Processar as Configurações para cada Produto
            foreach (var configCategory in allConfigsForModel)
            {
                var finalOptionIds = await ResolveOptionIds(configCategory, dto.Configs);

                foreach (var optionId in finalOptionIds)
                {
                    await _productConfigRepo.Create(new ProductConfigModel
                    {
                        ProductId = product.Id,
                        ConfigOptionId = optionId
                    });
                }
            }

            summaryItems.Add(new ProductSummaryDTO(product.Id, product.SerialNumber, mo.ManufacturingOrderNumber));
        }

        return new CreateClientOrderResultDTO(createdOrder.Id, createdOrder.CustomerName, createdOrder.Quantity, summaryItems);
    }

    public async Task<bool> Update(int id, UpdateClientOrderDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return false;

        if (dto.OrderNumber != null) entity.OrderNumber = dto.OrderNumber;
        if (dto.OrderDate != null) entity.OrderDate = dto.OrderDate.Value;
        if (dto.CustomerName != null) entity.CustomerName = dto.CustomerName;
        if (dto.Quantity != null) entity.Quantity = dto.Quantity.Value;

        await _repo.Update(entity);
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await _repo.Exists(id)) return false;
        await _repo.Delete(id);
        return true;
    }
}