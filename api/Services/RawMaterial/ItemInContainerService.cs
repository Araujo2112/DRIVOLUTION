using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;
using ApiTexPact.Services.RawMaterial.Interfaces.ItemInContainer;
using Microsoft.Extensions.Configuration;

namespace ApiTexPact.Services.RawMaterial
{
    public class ItemInContainerService : IItemInContainerService
    {
        private readonly IItemInContainerRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public ItemInContainerService(
            IItemInContainerRepository repository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<ItemInContainerModel> AddItemToContainerAsync(ItemInContainerModel item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item), "Item cannot be null.");

            if (item.DateTimeIn >= item.DateTimeOut)
                throw new ArgumentException("Entry date must be earlier than exit date.");


            item.ItemCode = $"temp-code-{DateTime.UtcNow.Ticks}";

            var newItem = await _repository.AddItemToContainerAsync(item);


            newItem.ItemCode = $"urn:ngsi-ld:ItemInContainer:{newItem.ItemInContainerId}";
            await _repository.UpdateItemInContainerAsync(newItem);

            try
            {
                await CreateItemOnFiwareAsync(newItem);
                return newItem;
            }
            catch (Exception fiwareEx)
            {
                await _repository.RemoveItemFromContainerAsync(newItem.ItemInContainerId);
                throw new Exception($"Error adding item to FIWARE: {fiwareEx.Message}. Item creation has been rolled back.");
            }
        }



        public async Task<List<ItemInContainerModel>> GetAllItemsInContainerAsync()
        {
            return await _repository.GetAllItemInContainerAsync();
        }

        public async Task<ItemInContainerModel> GetItemByAsync(int itemInContainerId)
        {
            if (itemInContainerId == 0)
                throw new ArgumentException("Item ID cannot be zero.", nameof(itemInContainerId));

            return await _repository.GetItemAsync(itemInContainerId);
        }

        public async Task<bool> RemoveItemFromContainerAsync(int itemInContainerId)
        {
            if (itemInContainerId == 0)
                throw new ArgumentException("Item ID cannot be zero.", nameof(itemInContainerId));

            var item = await _repository.GetItemAsync(itemInContainerId);
            if (item == null)
                return false;

            await DeleteItemFromFiwareAsync(itemInContainerId);
            return await _repository.RemoveItemFromContainerAsync(itemInContainerId);
        }

       private async Task UpdateItemOnFiwareAsync(ItemInContainerModel item)
{
    var client = _httpClientFactory.CreateClient();
    var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:ItemInContainer:{item.ItemInContainerId}/attrs";

    Console.WriteLine($"🌍 [FIWARE] PATCH URL: {url}");
    Console.WriteLine($"📦 [FIWARE] Payload enviado: {JsonSerializer.Serialize(item)}");

    var payload = new Dictionary<string, object>
    {
        ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
        ["id"] = $"urn:ngsi-ld:ItemInContainer:{item.ItemInContainerId}",
        ["type"] = "ItemInContainer",
        ["name"] = new { type = "Property", value = item.ItemCode },
        ["datetimeIn"] = new { type = "Property", value = item.DateTimeIn },
        ["datetimeOut"] = new { type = "Property", value = item.DateTimeOut },
        ["containerId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:Container:{item.ContainerId}" }
    };

    var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");

    var response = await client.PatchAsync(url, jsonContent);
    Console.WriteLine($"📥 [FIWARE] Resposta: {response.StatusCode} - {response.ReasonPhrase}");

    if (!response.IsSuccessStatusCode)
    {
        var responseBody = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"[FIWARE] Erro ao atualizar: {responseBody}");
        throw new Exception($"Error updating item in FIWARE: {response.StatusCode}, Response: {responseBody}");
    }
}

public async Task<ItemInContainerModel> UpdateItemInContainerAsync(ItemInContainerModel item)
{
    if (item == null)
        throw new ArgumentNullException(nameof(item), "Item cannot be null.");

    if (item.DateTimeIn >= item.DateTimeOut)
        throw new ArgumentException("Entry date must be earlier than exit date.");

    Console.WriteLine($"✏️ [UPDATE] Atualizando item localmente: {JsonSerializer.Serialize(item)}");

    var updatedItem = await _repository.UpdateItemInContainerAsync(item);

    try
    {
        Console.WriteLine("🔄 [UPDATE] Enviando atualização ao FIWARE...");
        await UpdateItemOnFiwareAsync(updatedItem);
    }
    catch (Exception fiwareEx)
    {
        Console.WriteLine($"🔥 [FIWARE] Erro na atualização: {fiwareEx.Message}");
        await _repository.UpdateItemInContainerAsync(updatedItem);
        throw new Exception($"Error updating item in FIWARE: {fiwareEx.Message}. Database update rolled back.");
    }

    Console.WriteLine("✅ [UPDATE] Atualização completa com sucesso.");
    return updatedItem;
}



        private async Task CreateItemOnFiwareAsync(ItemInContainerModel item)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities";

            var payload = new Dictionary<string, object>
            {
                ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
                ["id"] = $"urn:ngsi-ld:ItemInContainer:{item.ItemInContainerId}",
                ["type"] = "ItemInContainer",
                ["name"] = new { type = "Property", value = item.ItemCode ?? item.ItemCode?.ToString() },
                ["datetimeIn"] = new { type = "Property", value = item.DateTimeIn },
                ["datetimeOut"] = new { type = "Property", value = item.DateTimeOut },
                ["containerId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:Container:{item.ContainerId}" }
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

            var response = await client.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error creating item in FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }
        

        private async Task DeleteItemFromFiwareAsync(int itemInContainerId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:ItemInContainer:{itemInContainerId}";

            var response = await client.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception($"Error deleting item from FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }
    }
}
