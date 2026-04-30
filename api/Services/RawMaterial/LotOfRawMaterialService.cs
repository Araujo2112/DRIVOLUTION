using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiTexPact.DTO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ApiTexPact.Services
{
    public class LotOfRawMaterialService : ILotOfRawMaterialService
    {
        private readonly ILotOfRawMaterialRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LotOfRawMaterialService(
            ILotOfRawMaterialRepository repository,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<LotOfRawMaterialModel> CreateLotAsync(LotOfRawMaterialModel lotModel)
        {
            
            var existingLotByCode = await _repository.GetlotByCodeAsync(lotModel.LotId);
            if (existingLotByCode != null)
            {
                throw new Exception($"A lot with the same LotCode '{lotModel.LotCode}' already exists.");
            }

            // Definindo um código temporário para o lote
            lotModel.LotCode = $"temporary_code_{lotModel.LotId}"; 

            // Criação do modelo de lote com os dados fornecidos
            var lot = new LotOfRawMaterialModel
            {
                LotCode = lotModel.LotCode,
                LotNumber = lotModel.LotNumber,
                LotQuantity = lotModel.LotQuantity,
                LotUnit = lotModel.LotUnit,
                RawMaterialId = lotModel.RawMaterialId,
                HistoricalWeights = lotModel.HistoricalWeights
            };
            
            await _repository.CreatelotAsync(lot);
    
            try
            {
                
                await CreateLotOnFiwareAsync(lot);

         
                lot.LotCode = $"urn:ngsi-ld:LotOfRawMaterial:{lot.LotId}";

                await _repository.UpdatelotAsync(lot.LotId, lot.LotNumber, lot.LotCode, lot.LotQuantity, lot.LotUnit);
                
                return lot;
            }
            catch (Exception fiwareEx)
            {
           
                await _repository.DeletelotByCodeAsync(lot.LotId);

                
                throw new Exception($"Error creating lot in FIWARE: {fiwareEx.Message}. Lot creation in repository has been rolled back.");
            }
        }



        public async Task<List<LotOfRawMaterialModel>> GetAllLotsAsync()
        {
            return await _repository.GetAlllotsAsync();
        }

        public async Task<LotOfRawMaterialResponseDTO> GetLotByCodeAsync(int lotId)
        {
            var lot = await _repository.GetlotByCodeAsync(lotId);
            if (lot == null) return null;

            return new LotOfRawMaterialResponseDTO
            {
                LotId = lot.LotId,
                LotNumber = lot.LotNumber,
                LotQuantity = lot.LotQuantity,
                LotUnit = lot.LotUnit,
                RawMaterialId = lot.RawMaterialId,
                HistoricalWeights = lot.HistoricalWeights ?? new List<int>()
            };
        }

        public async Task<bool> DeleteLotByCodeAsync(int lotId)
        {
            return await _repository.DeletelotByCodeAsync(lotId);
        }

        public async Task<LotOfRawMaterialModel> UpdateLotAsync(int lotId, string lotNumber, string lotCode, int lotQuantity, string lotUnit)
        {
           
            var updatedLot = new LotOfRawMaterialModel
            {
                LotId = lotId,
                LotNumber = lotNumber,
                LotCode = lotCode,
                LotQuantity = lotQuantity,
                LotUnit = lotUnit
            };

            try
            {
               
                await UpdateLotOnFiwareAsync(updatedLot);

              
                var lotFromDb = await _repository.UpdatelotAsync(lotId, lotNumber, lotCode, lotQuantity, lotUnit);
        
                return lotFromDb;
            }
            catch (Exception fiwareEx)
            {
          
                throw new Exception($"Error updating lot in FIWARE: {fiwareEx.Message}");
            }
        }

        private async Task<LotOfRawMaterialModel> CreateLotOnFiwareAsync(LotOfRawMaterialModel lot)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities";

            var ngsiLdLot = new Dictionary<string, object>
            {
                { "@context", new[] { "https://schema.lab.fiware.org/ld/context" } },
                { "id", $"urn:ngsi-ld:Lot:{lot.LotId}" },
                { "type", "lot" },
                {
                    "https://uri.fiware.org/ns/data-models#number", new
                    {
                        type = "Property",
                        value = lot.LotNumber
                    }
                },
                {
                    "https://uri.fiware.org/ns/data-models#quantity", new
                    {
                        type = "Property",
                        value = lot.LotQuantity
                    }
                },
                {
                    "https://uri.fiware.org/ns/data-models#unit", new
                    {
                        type = "Property",
                        value = lot.LotUnit
                    }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(ngsiLdLot),
                Encoding.UTF8,
                "application/ld+json");

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/ld+json"));

            var response = await client.PostAsync(url, jsonContent);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Error creating lot in FIWARE: {response.StatusCode}, Response: {responseBody}");
            }

            return lot;
        }
        
        private async Task UpdateLotOnFiwareAsync(LotOfRawMaterialModel lot)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:Lot:{lot.LotId}/attrs";

            var payload = new Dictionary<string, object>
            {
                ["@context"] = new[]
                {
                    "https://schema.lab.fiware.org/ld/context"
                },
                ["https://uri.fiware.org/ns/data-models#number"] = new
                {
                    type = "Property",
                    value = lot.LotNumber
                },
                ["https://uri.fiware.org/ns/data-models#quantity"] = new
                {
                    type = "Property",
                    value = lot.LotQuantity
                },
                ["https://uri.fiware.org/ns/data-models#unit"] = new
                {
                    type = "Property",
                    value = lot.LotUnit
                },
                ["https://uri.fiware.org/ns/data-models#lastUpdate"] = new
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
                throw new Exception($"Error updating lot in FIWARE: {response.StatusCode}\n{error}");
            }
        }



        
        private async Task DeleteLotOnFiwareAsync(int lotId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:Lot:{lotId}";

            var response = await client.DeleteAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Error deleting lot from FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }


    }
}
