using ApiTexPact.Models;
using ApiTexPact.Service.Interface;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ApiTexPact.Data;

namespace ApiTexPact.Service
{
    public class ItemOfRawMaterialService : IItemOfRawMaterialService
    {
        private readonly IItemOfRawMaterialRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ItemOfRawMaterialService(
            IItemOfRawMaterialRepository repository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<List<ItemOfRawMaterialModel>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<ItemOfRawMaterialModel> GetByCodeAsync(int itemRawId)
        {
            return await _repository.GetByCodeAsync(itemRawId);
        }

        public async Task<ItemOfRawMaterialModel> CreateAsync(ItemOfRawMaterialDTO itemDto)
        {
            if (itemDto == null)
                throw new ArgumentNullException(nameof(itemDto), "Item cannot be null.");

            var existingItem = await _repository.GetByCodeAsync(itemDto.ItemRawId);
            if (existingItem != null)
                throw new Exception($"ItemOfRawMaterial with code {itemDto.ItemRawId} already exists.");

            var tempCode = $"temp-code-{DateTime.UtcNow.Ticks}";
            itemDto.ItemCode = tempCode;

            var created = await _repository.CreateAsync(itemDto);

            var finalCode = $"urn:ngsi-ld:ItemOfRawMaterial:{created.ItemRawId}";
            created.ItemCode = finalCode;
            
            var updatedDto = new ItemOfRawMaterialDTO
            {
                ItemRawId = created.ItemRawId,
                ItemCode = finalCode,
                Quantity = created.Quantity,
                Unit = created.Unit,
                LotOfRawMaterialId = created.LotOfRawMaterialId,
                ManufacturingOrderId = created.ManufacturingOrderId,
                ManufacturingOrderPhaseId = created.ManufacturingOrderPhaseId,
                ItemInContainerId = created.ItemInContainerId
            };

            await _repository.UpdateAsync(updatedDto);

            try
            {
                await CreateOnFiwareAsync(created);
                return created;
            }
            catch (Exception fiwareEx)
            {
                await _repository.DeleteByCodeAsync(created.ItemRawId);
                throw new Exception($"Error adding item to FIWARE: {fiwareEx.Message}. Item creation has been rolled back.");
            }
        }



        public async Task<ItemOfRawMaterialModel> UpdateAsync(ItemOfRawMaterialDTO itemDto)
        {
            if (itemDto == null)
                throw new ArgumentNullException(nameof(itemDto), "Item cannot be null.");

            var itemToUpdate = await _repository.GetByCodeAsync(itemDto.ItemRawId);
            if (itemToUpdate == null)
                throw new Exception($"ItemOfRawMaterial with code {itemDto.ItemRawId} not found.");
            
            itemToUpdate.Quantity = itemDto.Quantity;
            itemToUpdate.Unit = itemDto.Unit;
            itemToUpdate.LotOfRawMaterialId = itemDto.LotOfRawMaterialId;
            itemToUpdate.ManufacturingOrderId = itemDto.ManufacturingOrderId;
            itemToUpdate.ManufacturingOrderPhaseId = itemDto.ManufacturingOrderPhaseId;

            await UpdateOnFiwareAsync(itemToUpdate);

            var updatedItem = await _repository.UpdateAsync(itemDto);

            return updatedItem;
        }

        public async Task<bool> DeleteByCodeAsync(int itemRawId)
        {
            await DeleteOnFiwareAsync(itemRawId);
            return await _repository.DeleteByCodeAsync(itemRawId);
        }

        private async Task CreateOnFiwareAsync(ItemOfRawMaterialModel item)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities";

            var ngsiLdItem = new Dictionary<string, object>
            {
                { "@context", new[] { "https://schema.lab.fiware.org/ld/context" } },
                { "id", $"urn:ngsi-ld:ItemOfRawMaterial:{item.ItemRawId}" },
                { "type", "ItemOfRawMaterial" },
                { "https://uri.fiware.org/ns/data-models#quantity", new { type = "Property", value = item.Quantity } },
                { "https://uri.fiware.org/ns/data-models#unit", new { type = "Property", value = item.Unit } },
                { "https://uri.fiware.org/ns/data-models#lotOfRawMaterialId", new { type = "Relationship", @object = $"urn:ngsi-ld:LotOfRawMaterial:{item.LotOfRawMaterialId}" } },
                { "https://uri.fiware.org/ns/data-models#itemInContainerId", new { type = "Relationship", @object = $"urn:ngsi-ld:ItemInContainer:{item.ItemInContainerId}" } },
                { "https://uri.fiware.org/ns/data-models#manufacturingOrderId", new { type = "Relationship", @object = $"urn:ngsi-ld:ManufacturingOrder:{item.ManufacturingOrderId}" } },
                { "https://uri.fiware.org/ns/data-models#manufacturingOrderPhaseId", new { type = "Relationship", @object = $"urn:ngsi-ld:ManufacturingOrderPhase:{item.ManufacturingOrderPhaseId}" } }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(ngsiLdItem), Encoding.UTF8, "application/ld+json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));
            await client.PostAsync(url, jsonContent);
        }

        private async Task UpdateOnFiwareAsync(ItemOfRawMaterialModel item)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:ItemOfRawMaterial:{item.ItemRawId}/attrs";

            var ngsiLdUpdatePayload = new Dictionary<string, object>
            {
                { "https://uri.fiware.org/ns/data-models#quantity", new { type = "Property", value = item.Quantity } },
                { "https://uri.fiware.org/ns/data-models#unit", new { type = "Property", value = item.Unit } },
                { "https://uri.fiware.org/ns/data-models#lotOfRawMaterialId", new { type = "Relationship", @object = $"urn:ngsi-ld:LotOfRawMaterial:{item.LotOfRawMaterialId}" } },
                { "https://uri.fiware.org/ns/data-models#itemInContainerId", new { type = "Relationship", @object = $"urn:ngsi-ld:ItemInContainer:{item.ItemInContainerId}" } },
                { "https://uri.fiware.org/ns/data-models#manufacturingOrderId", new { type = "Relationship", @object = $"urn:ngsi-ld:ManufacturingOrder:{item.ManufacturingOrderId}" } },
                { "https://uri.fiware.org/ns/data-models#manufacturingOrderPhaseId", new { type = "Relationship", @object = $"urn:ngsi-ld:ManufacturingOrderPhase:{item.ManufacturingOrderPhaseId}" } }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(ngsiLdUpdatePayload), Encoding.UTF8, "application/json");
            await client.PatchAsync(url, jsonContent);
        }

        private async Task DeleteOnFiwareAsync(int itemRawId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:ItemOfRawMaterial:{itemRawId}";
            await client.DeleteAsync(url);
        }
    }
}