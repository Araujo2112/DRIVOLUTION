using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Product;
using ApiTexPact.Services.Interface.Product;
using Microsoft.Extensions.Configuration;

namespace ApiTexPact.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ProductService(IProductRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ProductDTO>> GetAllProducts()
    {
        var products = await _repository.GetAll();
        return products.Select(ToDTO);
    }

    public async Task<ProductDTO> GetProductById(int id)
    {
        var product = await _repository.GetById(id);
        return ToDTO(product);
    }

    public async Task<ProductDTO> CreateProduct(CreateProductDTO productDto)
    {
        if (productDto == null)
            throw new ArgumentNullException(nameof(productDto), "Product cannot be null.");

        productDto.ProductId = $"temp-code-{DateTime.UtcNow.Ticks}";

        var product = new ProductModel
        {
            Name = productDto.Name,
            Info = productDto.Info,
            ProductId = productDto.ProductId
        };

        var created = await _repository.Create(product); 

        created.ProductId = $"urn:ngsi-ld:Product:{created.Id}";
        productDto.ProductId = created.ProductId;
        
        created.ProductId = productDto.ProductId;
        await _repository.Update(created);

        try
        {
            await CreateProductOnFiwareAsync(created);
            return ToDTO(created);
        }
        catch (Exception fiwareEx)
        {
            await _repository.Delete(created.Id);
            throw new Exception($"Error adding product to FIWARE: {fiwareEx.Message}. Product creation has been rolled back.");
        }
    }
    

    public async Task<ProductDTO> UpdateProduct(int id, UpdateProductDTO productDto)
    {
        var existing = await _repository.GetById(id);

        existing.Name = productDto.Name;
        existing.Info = productDto.Info;

        await _repository.Update(existing);

        await UpdateProductOnFiwareAsync(existing);

        return ToDTO(existing);
    }

    public async Task DeleteProduct(int id)
    {
        await _repository.Delete(id);
    }

    private static ProductDTO ToDTO(ProductModel product)
    {
        return new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Info = product.Info,
            ProductId = product.ProductId
        };
    }

    private async Task CreateProductOnFiwareAsync(ProductModel product)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
            ["id"] = product.ProductId,
            ["type"] = "Product",
            ["name"] = new { type = "Property", value = product.Name },
            ["info"] = new { type = "Property", value = product.Info }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

        var response = await client.PostAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE creation failed: {response.StatusCode}, {responseBody}");
        }
    }

    private async Task UpdateProductOnFiwareAsync(ProductModel product)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/{product.ProductId}/attrs";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" }, // ✅ Adiciona isto
            ["name"] = new { type = "Property", value = product.Name },
            ["info"] = new { type = "Property", value = product.Info }
        };

        var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");

        var response = await client.PatchAsync(url, jsonContent);

        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE update failed: {response.StatusCode}, {responseBody}");
        }
    }

}
