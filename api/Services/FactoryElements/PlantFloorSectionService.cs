using System.Text;
using System.Text.Json;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;


namespace ApiTexPact.Services.PlantFloorSection
{
    public class PlantFloorSectionService : IPlantFloorSectionService
    {
        private readonly IPlantFloorSectionRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public PlantFloorSectionService(IPlantFloorSectionRepository repository, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<PlantFloorSectionModel> CreateSectionAsync(PlantFloorSectionModel sectionModel)
        {
            if (sectionModel == null)
                throw new ArgumentNullException(nameof(sectionModel), "Section cannot be null.");

            var existingSection = await _repository.GetSectionById(sectionModel.SectionId);
            if (existingSection != null)
            {
                throw new Exception($"A section with the same ID '{sectionModel.SectionId}' already exists.");
            }

            sectionModel.SectionCode = $"temporary_code_{sectionModel.SectionId}";

            var section = new PlantFloorSectionModel
            {
                name = sectionModel.name, 
                SectionCode = sectionModel.SectionCode
            };

            await _repository.CreatePlantFloorSectionAsync(section);
            
            section.name = $"Section {section.SectionId}";
            await _repository.UpdatePlantFloorSectionAsync(section.SectionId, section.name);

            await CreateSectionOnFiwareAsync(section);

            section.SectionCode = $"urn:ngsi-ld:PlantFloorSection:{section.SectionId}";
            await _repository.UpdatePlantFloorSectionAsync(section.SectionId, section.SectionCode);

            return section;
        }





        public async Task<List<PlantFloorSectionModel>> GetAllSectionsAsync()
        {
            return await _repository.GetAllPlantFloorSectionsAsync();
        }

        public async Task<PlantFloorSectionModel> GetSectionById(int sectionId)
        {
            if (sectionId <= 0)
            {
                throw new ArgumentException("Section code cannot be null or empty.", nameof(sectionId));
            }

            return await _repository.GetSectionById(sectionId);
        }
        
        public async Task<PlantFloorSectionModel> GetSectionByCodeAsync(string sectionCode)
        {
            if (sectionCode == null)
            {
                throw new ArgumentException("Section code cannot be null or empty.", nameof(sectionCode));
            }

            return await _repository.GetSectionByCodeAsync(sectionCode);
        }


        public async Task<PlantFloorSectionModel> UpdateSectionAsync(int sectionId, string name)
        {
            if (sectionId <= 0)
            {
                throw new ArgumentException("Section code cannot be null or empty.", nameof(sectionId));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Section name cannot be null or empty.", nameof(name));
            }
           
            await UpdateSectionOnFiwareAsync(sectionId, name);
            
            var updatedSection = await _repository.UpdatePlantFloorSectionAsync(sectionId, name);
            return updatedSection;
        }

        public async Task<bool> DeleteSectionByCodeAsync(int sectionId)
        {
            if (sectionId <= 0)
            {
                throw new ArgumentException("Section code cannot be null or empty.", nameof(sectionId));
            }

        
            var isDeleted = await _repository.DeletePlantFloorSectionByCodeAsync(sectionId);

            
            if (isDeleted)
            {
                await DeleteSectionFromFiwareAsync(sectionId);
            }

            return isDeleted;
        }

    
        private async Task CreateSectionOnFiwareAsync(PlantFloorSectionModel section)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities";

            var ngsiLdSection = new Dictionary<string, object>
            {
                { "@context", new[] { "https://schema.lab.fiware.org/ld/context" } },
                { "id", $"urn:ngsi-ld:PlantFloorSection:{section.SectionId}" },
                { "type", "PlantFloorSection" },
                {
                    "https://uri.fiware.org/ns/data-models#code", new
                    {
                        type = "Property",
                        value = section.SectionId
                    }
                },
                {
                    "https://uri.fiware.org/ns/data-models#name", new
                    {
                        type = "Property",
                        value = section.name 
                    }
                }
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(ngsiLdSection),
                Encoding.UTF8,
                "application/ld+json");

            var response = await client.PostAsync(url, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Error creating section in FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }

       
        private async Task UpdateSectionOnFiwareAsync(int sectionId, string name)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:PlantFloorSection:{sectionId}/attrs";

            var ngsiLdUpdatePayload = new Dictionary<string, object>
            {
                {
                    "https://uri.fiware.org/ns/data-models#code", new
                    {
                        type = "Property",
                        value = sectionId
                    }
                },
                {
                    "https://uri.fiware.org/ns/data-models#name", new
                    {
                        type = "Property",
                        value = name
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
                    $"Error updating section in FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }

 
        private async Task DeleteSectionFromFiwareAsync(int sectionId)
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_configuration["FiwareServiceUrl"]}/entities/urn:ngsi-ld:PlantFloorSection:{sectionId}";

            var response = await client.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new Exception(
                    $"Error deleting section in FIWARE: {response.StatusCode}, Response: {responseBody}");
            }
        }
    }
}
