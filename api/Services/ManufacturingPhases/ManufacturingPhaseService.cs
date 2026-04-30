using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingPhase;
using ApiTexPact.Services.Interface.ManufacturingPhase;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ApiTexPact.Services;

public class ManufacturingPhaseService : IManufacturingPhaseService
{
    private readonly IManufacturingPhaseRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public ManufacturingPhaseService(
        IManufacturingPhaseRepository repository,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task<IEnumerable<ManufacturingPhaseDTO>> GetAllManufacturingPhases()
    {
        var phases = await _repository.GetAll();
        return phases.Select(ToDTO);
    }

    public async Task<ManufacturingPhaseDTO> GetManufacturingPhaseById(int id)
    {
        var phase = await _repository.GetById(id);
        return ToDTO(phase);
    }

    public async Task<ManufacturingPhaseDTO> CreateManufacturingPhase(CreateManufacturingPhaseDTO dto)
    {
        var tempId = $"temp-code-{DateTime.UtcNow.Ticks}";

        var phase = new ManufacturingPhaseModel
        {
            ManufacturingPhaseId = tempId,
            PhaseInfo = dto.PhaseInfo,
            PhaseDuration = dto.PhaseDuration,
            PlantFloorSectionId = dto.PlantFloorSectionId
        };

        var created = await _repository.Create(phase);

        created.ManufacturingPhaseId = $"urn:ngsi-ld:ManufacturingPhase:{created.Id}";
        await _repository.Update(created);

        try
        {
            await CreateOnFiwareAsync(created);
            return ToDTO(created);
        }
        catch (Exception ex)
        {
            await _repository.Delete(created.Id);
            throw new Exception($"Error adding ManufacturingPhase to FIWARE: {ex.Message}");
        }
    }

    public async Task<ManufacturingPhaseDTO> UpdateManufacturingPhase(int id, UpdateManufacturingPhaseDTO dto)
    {
        var existing = await _repository.GetById(id);

        existing.PhaseInfo = dto.PhaseInfo;
        existing.PhaseDuration = dto.PhaseDuration;

        await _repository.Update(existing);
        await UpdateOnFiwareAsync(existing);

        return ToDTO(existing);
    }

    public async Task DeleteManufacturingPhase(int id)
    {
        var existing = await _repository.GetById(id);
        if (existing != null)
        {
            await DeleteOnFiwareAsync(existing.ManufacturingPhaseId);
            await _repository.Delete(id);
        }
    }

    private async Task CreateOnFiwareAsync(ManufacturingPhaseModel phase)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
            ["id"] = phase.ManufacturingPhaseId,
            ["type"] = "ManufacturingPhase",
            ["phaseInfo"] = new { type = "Property", value = phase.PhaseInfo },
            ["phaseDuration"] = new { type = "Property", value = phase.PhaseDuration },
            ["plantFloorSectionId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:PlantFloorSection:{phase.PlantFloorSectionId}" }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

        var response = await client.PostAsync(url, content);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE creation failed: {response.StatusCode}, {body}");
        }
    }

    private async Task UpdateOnFiwareAsync(ManufacturingPhaseModel phase)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/{phase.ManufacturingPhaseId}/attrs";

        var payload = new Dictionary<string, object>
        {
            ["@context"] = new[] { "https://uri.etsi.org/ngsi-ld/v1/ngsi-ld-core-context.jsonld" },
            ["phaseInfo"] = new { type = "Property", value = phase.PhaseInfo },
            ["phaseDuration"] = new { type = "Property", value = phase.PhaseDuration },
            ["plantFloorSectionId"] = new { type = "Relationship", @object = $"urn:ngsi-ld:PlantFloorSection:{phase.PlantFloorSectionId}" }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/ld+json");
        var response = await client.PatchAsync(url, content);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE update failed: {response.StatusCode}, {body}");
        }
    }

    private async Task DeleteOnFiwareAsync(string phaseId)
    {
        var client = _httpClientFactory.CreateClient();
        var url = $"{_configuration["FiwareServiceUrl"]}/entities/{phaseId}";
        await client.DeleteAsync(url);
    }

    private static ManufacturingPhaseDTO ToDTO(ManufacturingPhaseModel phase)
    {
        return new ManufacturingPhaseDTO
        {
            Id = phase.Id,
            PhaseInfo = phase.PhaseInfo,
            PhaseDuration = phase.PhaseDuration,
            ManufacturingPhaseId = phase.ManufacturingPhaseId,
            PlantFloorSectionId = phase.PlantFloorSectionId,
        };
    }
}
