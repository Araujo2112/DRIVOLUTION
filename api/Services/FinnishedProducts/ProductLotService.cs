using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ProductLot;
using ApiTexPact.Services.Interface.ProductLot;
using Microsoft.Extensions.Configuration;

namespace ApiTexPact.Services;

public class ProductLotService : IProductLotService
{
    private readonly IProductLotRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ProductLotService(IProductLotRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ProductLotDTO>> GetAllProductLots()
    {
        var productLots = await _repository.GetAll();
        return productLots.Select(ToDTO);
    }

    public async Task<ProductLotDTO> GetProductLotById(int id)
    {
        var productLot = await _repository.GetById(id);
        return ToDTO(productLot);
    }

    public async Task<ProductLotDTO> CreateProductLot(CreateProductLotDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto), "ProductLot cannot be null.");

        dto.ProductLotId = $"temp-code-{DateTime.UtcNow.Ticks}";

        var productLot = new ProductLotModel
        {
            LotNumber = dto.ProductLotId, 
            LotUnit = dto.LotUnit,
            LotQuantity = dto.LotQuantity,
            Ready = dto.Ready,
            ProductLotId = dto.ProductLotId,
            ProductId = dto.ProductId
        };

        var created = await _repository.Create(productLot);
        
        created.ProductLotId = $"urn:ngsi-ld:ProductLot:{created.Id}";
        created.LotNumber = $"urn:ngsi-ld:ProductLot:{created.Id}";
        created.LotNumber = created.ProductLotId;
        dto.ProductLotId = created.ProductLotId;

        await _repository.Update(created);

        try
        {
            await CreateProductLotOnFiwareAsync(created);
            return ToDTO(created);
        }
        catch (Exception ex)
        {
            await _repository.Delete(created.Id); 
            throw new Exception($"Error adding ProductLot to FIWARE: {ex.Message}");
        }
    }



    public async Task<ProductLotDTO> UpdateProductLot(int id, UpdateProductLotWithIdDTO dto)
    {
        var existing = await _repository.GetById(id);
        
        existing.LotUnit = dto.LotUnit;
        existing.LotQuantity = dto.LotQuantity;
        existing.Ready = dto.Ready;
        existing.ProductId = dto.ProductId;

        await _repository.Update(existing);

        await UpdateProductLotOnFiwareAsync(existing);

        return ToDTO(existing);
    }

    public async Task DeleteProductLot(int id)
    {
        await _repository.Delete(id);
        await DeleteFromFiwareAsync(id);
    }

    private static ProductLotDTO ToDTO(ProductLotModel productLot)
    {
        return new ProductLotDTO
        {
            Id = productLot.Id,
            LotNumber = productLot.LotNumber,
            LotUnit = productLot.LotUnit,
            LotQuantity = productLot.LotQuantity,
            Ready = productLot.Ready,
            ProductLotId = productLot.ProductLotId,
            ProductName = productLot.Product?.Name,
            Info = productLot.Product?.Info,
            ProductId = productLot.Product?.ProductId
            
        };
    }

    private async Task CreateProductLotOnFiwareAsync(ProductLotModel productLot)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
            ["id"] = $"urn:ngsi-ld:ProductLot:{productLot.Id}",
            ["type"] = "ProductLot",
            ["lotNumber"] = new { type = "Property", value = productLot.LotNumber },
            ["lotUnit"] = new { type = "Property", value = productLot.LotUnit },
            ["lotQuantity"] = new { type = "Property", value = productLot.LotQuantity },
            ["ready"] = new { type = "Property", value = productLot.Ready },
            ["productId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:Product:{productLot.ProductId}" }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

        var response = await client.PostAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE creation failed: {response.StatusCode}, {error}");
        }
    }

    private async Task UpdateProductLotOnFiwareAsync(ProductLotModel productLot)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:ProductLot:{productLot.Id}/attrs";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
            ["lotNumber"] = new { type = "Property", value = productLot.LotNumber },
            ["lotUnit"] = new { type = "Property", value = productLot.LotUnit },
            ["lotQuantity"] = new { type = "Property", value = productLot.LotQuantity },
            ["ready"] = new { type = "Property", value = productLot.Ready },
            ["productId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:Product:{productLot.ProductId}" }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");

        var response = await client.PatchAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE update failed: {response.StatusCode}, {error}");
        }
    }

    private async Task DeleteFromFiwareAsync(int productLotId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:ProductLot:{productLotId}";
        await client.DeleteAsync(url);
    }
}
