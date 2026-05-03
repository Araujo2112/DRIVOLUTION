using System.Text;
using System.Text.Json;

namespace ApiTexPact.Services;

public class OrionService
{
    private readonly HttpClient _http;

    public OrionService(HttpClient http)
    {
        _http = http;
    }

    public async Task UpsertProductAsync(int productId, string phase, int workstationId, string status)
    {
        var entity = new
        {
            id = $"urn:ngsi-ld:Product:{productId}",
            type = "Product",
            currentPhase = new
            {
                type = "Property",
                value = phase
            },
            workstationId = new
            {
                type = "Property",
                value = workstationId
            },
            status = new
            {
                type = "Property",
                value = status
            },
            updatedAt = new
            {
                type = "Property",
                value = DateTime.UtcNow
            }
        };

        var json = JsonSerializer.Serialize(entity);

        var request = new HttpRequestMessage(
            HttpMethod.Post,
            "http://orion:1026/ngsi-ld/v1/entityOperations/upsert"
        );

        request.Headers.Add("Accept", "application/ld+json");
        request.Content = new StringContent(
            $"[{json}]",
            Encoding.UTF8,
            "application/ld+json"
        );

        var response = await _http.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"FIWARE Orion error: {response.StatusCode} - {error}");
        }
    }
}