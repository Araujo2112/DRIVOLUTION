using System.Text;
using System.Text.Json;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface.Containers;

namespace ApiTexPact.Services.FactoryElements;

public class ContainerService : IContainerService
{
    private readonly IContainerRepository _containerRepository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ContainerService(
        IContainerRepository containerRepository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _containerRepository = containerRepository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<List<ContainerModel>> GetAllContainersAsync()
    {
        return await _containerRepository.GetAllContainersAsync();
    }

    public async Task<ContainerModel> GetContainerByCodeAsync(string containerCode)
    {
        var container = await _containerRepository.GetContainerByCodeAsync(containerCode);
        return container ?? throw new Exception($"Container {containerCode} not found.");
    }
    
    public async Task<ContainerModel> GetContainerId(int containerId)
    {
        var container = await _containerRepository.GetContainerById(containerId);
        return container ?? throw new Exception($"Container {containerId} not found.");
    }

    public async Task<ContainerModel> CreateContainerAsync(ContainerModel containerModel)
    {
        
        var existingContainerById = await _containerRepository.GetContainerById(containerModel.ContainerId);
        if (existingContainerById != null)
        {
            throw new Exception($"A container with the same ID '{containerModel.ContainerId}' already exists.");
        }
        
        containerModel.ContainerCode = $"temporary_code_{containerModel.ContainerId}"; 
  
        var container = new ContainerModel
        {
            ContainerName = containerModel.ContainerName,
            ContainerVolume = containerModel.ContainerVolume,
            Activate = containerModel.Activate,
            ContainerCode = containerModel.ContainerCode
        };
        
        await _containerRepository.AddContainerAsync(container);
        
        await CreateContainerOnFiwareAsync(container);
        
        container.ContainerCode = $"urn:ngsi-ld:Container:{container.ContainerId}";

        await _containerRepository.UpdateContainerAsync(container);
    
        return container;
    }

    public async Task<ContainerModel> UpdateContainerAsync(int containerId, string containerName, float containerVolume, bool activate)
    {
        var container = await _containerRepository.GetContainerById(containerId)
                        ?? throw new Exception($"Container {containerId} not found.");

        container.ContainerName = containerName;
        container.ContainerVolume = containerVolume;
        container.Activate = activate;

        await _containerRepository.UpdateContainerAsync(container);
        await UpdateContainerOnFiwareAsync(container);
        return container;
    }

    
    public async Task<bool> DeleteContainerByCodeAsync(int containerId)
    {
        var container = await _containerRepository.GetContainerById(containerId);
        if (container == null)
        {
            return false;
        }

        await DeleteContainerOnFiwareAsync(containerId);
        await _containerRepository.DeleteContainerAsync(containerId);
        return true;
    }

    private async Task CreateContainerOnFiwareAsync(ContainerModel container)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[]
            {
                "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld",
        
            },
            ["id"] = $"urn:ngsi-ld:Container:{container.ContainerId}",
            ["type"] = "https://uri.fiware.org/ns/data-models#Container",
            ["https://uri.etsi.org/ngsi-ld/name"] = new
            {
                type = "Property",
                value = container.ContainerName
            },
            ["https://uri.etsi.org/ngsi-ld/default-context/capacity"] = new
            {
                type = "Property",
                value = container.ContainerVolume
            },
            ["https://uri.etsi.org/ngsi-ld/default-context/activate"] = new
            {
                type = "Property",
                value = container.Activate
            },
            ["https://uri.etsi.org/ngsi-ld/default-context/lastUpdate"] = new
            {
                type = "Property",
                value = DateTime.UtcNow.ToString("o")
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/ld+json");

        var response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error creating container in FIWARE: {response.StatusCode}\n{error}");
        }
    }
    

    private async Task UpdateContainerOnFiwareAsync(ContainerModel container)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:Container:{container.ContainerId}/attrs";

        var payload = new Dictionary<string, object>
        {
            
            ["@context"] = new[]
            {
                "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld",
        
            },
            ["https://uri.etsi.org/ngsi-ld/name"] = new
            {
                type = "Property",
                value = container.ContainerName
            },
            ["https://uri.etsi.org/ngsi-ld/default-context/capacity"] = new
            {
                type = "Property",
                value = container.ContainerVolume
            },
            ["https://uri.etsi.org/ngsi-ld/default-context/activate"] = new
            {
                type = "Property",
                value = container.Activate
            },
            ["https://uri.etsi.org/ngsi-ld/default-context/lastUpdate"] = new
            {
                type = "Property",
                value = DateTime.UtcNow.ToString("o")
            }
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/ld+json");

        var response = await client.PatchAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error updating container in FIWARE: {response.StatusCode}\n{error}");
        }
    }


    private async Task DeleteContainerOnFiwareAsync(int containerId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:Container:{containerId}";

        var response = await client.DeleteAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error deleting container in FIWARE: {response.StatusCode}\n{error}");
        }
    }

}
