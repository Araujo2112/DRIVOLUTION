using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;

namespace ApiTexPact.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IContainerRepository _containerRepository;
        private readonly IPlantFloorSectionRepository _plantFloorSectionRepository;
        private readonly IContainerLocalizationRepository _containerLocalizationRepository;
        private readonly IItemOfRawMaterialRepository _itemOfRawMaterialRepository;
        private readonly IItemLocalizationRepository _itemLocalizationRepository;
        private readonly HttpClient _httpClient;

        public NotificationService(
            IContainerRepository containerRepository,
            IPlantFloorSectionRepository plantFloorSectionRepository,
            IContainerLocalizationRepository containerLocalizationRepository,
            IItemOfRawMaterialRepository itemOfRawMaterialRepository,
            IItemLocalizationRepository itemLocalizationRepository,
            HttpClient httpClient)
        {
            _containerRepository = containerRepository;
            _plantFloorSectionRepository = plantFloorSectionRepository;
            _containerLocalizationRepository = containerLocalizationRepository;
            _itemOfRawMaterialRepository = itemOfRawMaterialRepository;
            _itemLocalizationRepository = itemLocalizationRepository;
            _httpClient = httpClient;
        }

       public async Task<bool> ProcessNotification(JsonElement notification)
{
    Console.WriteLine("Notification received:");
    Console.WriteLine(notification.ToString());

    if (!notification.TryGetProperty("data", out JsonElement dataArray) || dataArray.ValueKind != JsonValueKind.Array)
    {
        Console.WriteLine("Invalid notification: 'data' not found or is not an array.");
        return false;
    }

    foreach (JsonElement sensor in dataArray.EnumerateArray())
    {
        Console.WriteLine("Processing sensor:");
        Console.WriteLine(sensor.ToString());

        if (sensor.TryGetProperty("tagDetected", out JsonElement tagDetectedProp) &&
            tagDetectedProp.TryGetProperty("value", out JsonElement tagDetectedValue) &&
            tagDetectedValue.GetBoolean())  
        {
            Console.WriteLine("Tag detected is true, processing container and location.");

            if (sensor.TryGetProperty("refContainer", out JsonElement refContainer) &&
                refContainer.TryGetProperty("object", out JsonElement containerCodeElement))
            {
                string containerCode = containerCodeElement.GetString();
                Console.WriteLine($"Container code: {containerCode}");

                var container = await _containerRepository.GetContainerByCodeAsync(containerCode);
                if (container == null)
                {
                    Console.WriteLine($"Container {containerCode} not found.");
                    continue;  
                }

                string newSection = string.Empty;
                if (sensor.TryGetProperty("refLocation", out JsonElement refLocation))
                {
                    if (refLocation.TryGetProperty("object", out JsonElement refLocationValue))
                    {
                        newSection = refLocationValue.GetString();
                        Console.WriteLine($"New section: {newSection}");
                    }
                }

                if (string.IsNullOrEmpty(newSection))
                {
                    Console.WriteLine("RefLocation not found or invalid.");
                    continue;
                }

                var sector = await _plantFloorSectionRepository.GetSectionByCodeAsync(newSection);
                if (sector == null)
                {
                    Console.WriteLine($"Section {newSection} not found.");
                    continue;  
                }
                
                var newLocalization = new ContainerLocalizationModel
                {
                    ContainerId = container.ContainerId,
                    SectionId = sector.SectionId, 
                    Datetime = DateTime.UtcNow
                };

                await _containerLocalizationRepository.CreateContainerLocalizationAsync(newLocalization);
                
                if (sensor.TryGetProperty("refRawMaterials", out JsonElement refRawMaterials) &&
                    refRawMaterials.TryGetProperty("object", out JsonElement rawMaterialsArray) &&
                    rawMaterialsArray.ValueKind == JsonValueKind.Array)
                {
                    foreach (var rawMaterial in rawMaterialsArray.EnumerateArray())
                    {
                        string rawIdUrn = rawMaterial.GetString();
                        int rawMaterialId = int.Parse(rawIdUrn.Split(':').Last());

                        var itemOfRawMaterial = await _itemOfRawMaterialRepository.GetByCodeAsync(rawMaterialId);
                        if (itemOfRawMaterial != null)
                        {
                            await _itemLocalizationRepository.CreateItemLocalizationAsync(new ItemLocalizationModel
                            {
                                ItemRawId = rawMaterialId,
                                ContainerLocalizationId = newLocalization.Id,
                                DateTime = DateTime.UtcNow
                            });
                            Console.WriteLine($"ItemOfRawMaterial {rawMaterialId} associado ao ContainerLocalization.");
                        }
                        else
                        {
                            Console.WriteLine($"ItemOfRawMaterial {rawMaterialId} não encontrado.");
                        }
                    }
                }
            }
        }
    }

    return true;
}

    }
}
