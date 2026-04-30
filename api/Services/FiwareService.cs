using System.Net.Http.Json;
using System.Threading.Tasks;
using ApiTextPact.Models;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace ApiTextPact.Services
{
    public class FiwareService : IFiwareService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public FiwareService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        // Método para atualizar o setor de um contêiner
        public async Task<bool> UpdateContainerSector(string containerId, string newSectorId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/containers/{containerId}";
            var payload = new { sectorId = newSectorId };
            
            var response = await client.PatchAsJsonAsync(url, payload);
            return response.IsSuccessStatusCode;
        }

        // Método para subscrever aos eventos de sensor de movimento
        public async Task<bool> SubscribeToSensorEvents(string porticoId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/subscriptions";
            var payload = new
            {
                description = "Notificar mudança de estado de sensores",
                subject = new { entities = new[] { new { id = porticoId, type = "Portico" } } },
                notification = new { http = new { url = $"{_configuration["BaseUrl"]}/api/container/move-container" }, attrs = new[] { "motionDetected" } },
                expires = "2024-11-20T12:00:00Z"
            };

            var response = await client.PostAsJsonAsync(url, payload);
            return response.IsSuccessStatusCode;
        }

        // Método para obter o estado do sensor de movimento
        public async Task<string> GetSensorState(string porticoId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/porticos/{porticoId}";
            var response = await client.GetFromJsonAsync<PorticoModel>(url);

            // Verificando se o portico existe e tem sensores de movimento
            if (response != null && response.MotionSensor != null && response.MotionSensor.Any())
            {
                // Acessa o status do primeiro sensor de movimento, por exemplo
                var motionSensor = response.MotionSensor.FirstOrDefault();
                return motionSensor?.Status;
            }
            
            return null;
        }
    }
}
