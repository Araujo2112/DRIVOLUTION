using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace ApiTexPact.Services
{
    public class CheckpointService : ICheckpointService
    {
        private readonly ICheckpointRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public CheckpointService(ICheckpointRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<List<CheckpointModel>> GetAllCheckpointsAsync()
        {
            return await _repository.GetAllCheckpointsFromDbAsync();
        }

        public async Task<CheckpointModel> GetCheckpointByIdAsync(int checkpointId)
        {
            if (checkpointId == 0)
            {
                throw new ArgumentException("O CheckpointId não pode ser 0.");
            }

            return await _repository.GetCheckpointByIdAsync(checkpointId);
        }

        public async Task<CheckpointModel> CreateCheckpointAsync(CheckpointModel checkpoint)
        {
            if (checkpoint == null)
                throw new ArgumentNullException(nameof(checkpoint));
            
            checkpoint.CheckpointId = 0; 
            
            var checkpointToSave = new CheckpointModel
            {
                CheckpointCode = $"temporary_code_{Guid.NewGuid()}",
                Name = checkpoint.Name,
                SectionId = checkpoint.SectionId,
                Status = checkpoint.Status,
            };
            
            await _repository.CreateCheckpointAsync(checkpointToSave);
            
            await CreateCheckpointOnFiwareAsync(checkpointToSave);
            
            checkpointToSave.CheckpointCode = $"urn:ngsi-ld:Checkpoint:{checkpointToSave.CheckpointId}";
            await _repository.UpdateCheckpointAsync(
                checkpointToSave.CheckpointId,
                checkpointToSave.Name,
                checkpointToSave.Status,
                checkpointToSave.SectionId
            );

            return checkpointToSave;
        }



        public async Task<CheckpointModel> UpdateCheckpointAsync(
            int checkpointId,
            string name,
            bool status,
            int sectionId)
        {
         
            await UpdateCheckpointOnFiwareAsync(checkpointId, name, status);
            
            var updatedCheckpoint = await _repository.UpdateCheckpointAsync(
                checkpointId,
                name,
                status,
                sectionId);

            return updatedCheckpoint;
        }


        public async Task<bool> DeleteCheckpointAsync(int checkpointId)
        {
  
            var isDeleted = await _repository.DeleteCheckpointByIdAsync(checkpointId);


            if (isDeleted)
            {
                await DeleteCheckpointFromFiwareAsync(checkpointId);
            }

            return isDeleted;
        }


        private async Task CreateCheckpointOnFiwareAsync(CheckpointModel checkpoint)
        {
            if (checkpoint == null)
                throw new ArgumentNullException(nameof(checkpoint), "Checkpoint is null.");

            if (string.IsNullOrWhiteSpace(checkpoint.Name))
                throw new ArgumentException("Checkpoint name is required.", nameof(checkpoint.Name));

            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities";

            var ngsiLdCheckpoint = new Dictionary<string, object>
            {
                { "@context", new[] { "https://schema.lab.fiware.org/ld/context" } },
                { "id", $"urn:ngsi-ld:Checkpoint:{checkpoint.CheckpointId}" },
                { "type", "Checkpoint" },
                {
                    "https://uri.fiware.org/ns/data-models#name", new
                    {
                        type = "Property",
                        value = checkpoint.Name
                    }
                },
                {
                    "https://uri.etsi.org/ngsi-ld/default-context/status", new
                    {
                        type = "Property",
                        value = checkpoint.Status
                    }
                },
                {
                    "https://uri.fiware.org/ns/data-models#sectionId", new
                    {
                        type = "Relationship",
                        @object = $"urn:ngsi-ld:Section:{checkpoint.SectionId}"
                    }
                }
                
            };

            var jsonPayload = JsonSerializer.Serialize(ngsiLdCheckpoint, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            Console.WriteLine("Payload send FIWARE (CREATE):\n" + jsonPayload);

            var jsonContent = new StringContent(jsonPayload, Encoding.UTF8, "application/ld+json");

            var response = await client.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine("FIWARE CREATE Response Body:\n" + responseBody);

                throw new Exception(
                    $"Error creating checkpoint on FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
            
        }


     
        private async Task UpdateCheckpointOnFiwareAsync(int checkpointId, string name, bool status)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:Checkpoint:{checkpointId}/attrs";

            var ngsiLdUpdatePayload = new Dictionary<string, object>
            {
                {
                    "https://uri.fiware.org/ns/data-models#name", new
                    {
                        type = "Property",
                        value = name
                    }
                },
                {
                    "https://uri.etsi.org/ngsi-ld/default-context/status", new
                    {
                        type = "Property",
                        value = status
                    }
                },
                {
                    "https://uri.fiware.org/ns/data-models#sectionId", new
                    {
                        type = "Relationship",
                        @object = $"urn:ngsi-ld:Section:{checkpointId}"
                    }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(ngsiLdUpdatePayload),
                Encoding.UTF8,
                "application/json");

            var response = await client.PatchAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Error updating checkpoint in FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }

        
        private async Task DeleteCheckpointFromFiwareAsync(int checkpointId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:Checkpoint:{checkpointId}";

            var response = await client.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Error deleting checkpoint in FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }
    }
}
