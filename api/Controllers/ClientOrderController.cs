using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ClientOrder;
using ApiTexPact.Repository.Interface.ManufacturingOrder;
using ApiTexPact.Repository.Interface.Product;
using ApiTexPact.Repository.Interface.CarModel;
using ApiTexPact.Repository.Interface.Config;
using ApiTexPact.Repository.Interface.ProductConfig;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientOrderController : ControllerBase
{
    private readonly IClientOrderRepository _repo;
    private readonly IManufacturingOrderRepository _manufacturingOrderRepo;
    private readonly IProductRepository _productRepo;
    private readonly ICarModelRepository _carModelRepo;
    private readonly IProductConfigRepository _productConfigRepo;
    private readonly IConfigRepository _configRepo;
    private readonly IConfigOptionRepository _configOptionRepo;

    public ClientOrderController(
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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(c => new ClientOrderDTO(c.Id, c.OrderNumber, c.OrderDate, c.CustomerName, c.Quantity)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ClientOrderDTO(item.Id, item.OrderNumber, item.OrderDate, item.CustomerName, item.Quantity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientOrderDTO dto)
    {
        // 1. Validar existência do modelo de carro
        var carModel = await _carModelRepo.GetById(dto.ModelId);
        if (carModel == null) return NotFound("Modelo de carro não encontrado.");

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
        var summaryItems = new List<object>();

        // 4. Gerar os itens físicos (Products) e Ordens de Fabrico (MOs)
        for (int i = 1; i <= dto.Quantity; i++)
        {
            // Criar Manufacturing Order individual
            var mo = await _manufacturingOrderRepo.Create(new ManufacturingOrderModel
            {
                ClientOrderId = createdOrder.Id,
                ManufacturingOrderNumber = $"{dto.OrderNumber}-MO-{i:D3}",
                StartDate = DateTime.UtcNow,
                Status = "pending"
            });

            // Criar o Produto (o veículo físico)
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
                int finalOptionId = 0;

                // Verificar se o cliente enviou uma escolha que pertença a esta categoria
                if (dto.Configs != null && dto.Configs.Any())
                {
                    foreach (var selection in dto.Configs)
                    {
                        var option = await _configOptionRepo.GetById(selection.ConfigOptionId);
                        // Se a opção existe e pertence à categoria que estamos a processar
                        if (option != null && option.ConfigId == configCategory.Id)
                        {
                            finalOptionId = option.Id;
                            break;
                        }
                    }
                }

                // Se não houve escolha do cliente para esta categoria, procurar a opção DEFAULT
                if (finalOptionId == 0)
                {
                    var defaultOption = await _configOptionRepo.GetDefaultByConfigId(configCategory.Id);
                    if (defaultOption != null)
                    {
                        finalOptionId = defaultOption.Id;
                    }
                }

                // Se encontrarmos uma opção (escolhida ou default), gravamos a ProductConfig
                if (finalOptionId != 0)
                {
                    await _productConfigRepo.Create(new ProductConfigModel
                    {
                        ProductId = product.Id,
                        ConfigOptionId = finalOptionId
                    });
                }
            }

            summaryItems.Add(new { productId = product.Id, serialNumber = product.SerialNumber, moNumber = mo.ManufacturingOrderNumber });
        }

        return CreatedAtAction(nameof(GetById), new { id = createdOrder.Id }, new
        {
            orderId = createdOrder.Id,
            customer = createdOrder.CustomerName,
            totalQuantity = createdOrder.Quantity,
            productsCreated = summaryItems
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientOrderDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();

        if (dto.OrderNumber != null) entity.OrderNumber = dto.OrderNumber;
        if (dto.OrderDate != null) entity.OrderDate = dto.OrderDate.Value;
        if (dto.CustomerName != null) entity.CustomerName = dto.CustomerName;
        if (dto.Quantity != null) entity.Quantity = dto.Quantity.Value;

        await _repo.Update(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _repo.Exists(id)) return NotFound();
        await _repo.Delete(id);
        return NoContent();
    }
}