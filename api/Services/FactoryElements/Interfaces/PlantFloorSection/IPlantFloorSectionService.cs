using ApiTexPact.Models;

namespace ApiTexPact.Services.PlantFloorSection
{
    public interface IPlantFloorSectionService
    {
        Task<PlantFloorSectionModel> CreateSectionAsync(PlantFloorSectionModel section);
        Task<List<PlantFloorSectionModel>> GetAllSectionsAsync();
        Task<PlantFloorSectionModel> GetSectionByCodeAsync(string sectionCode);
        Task<PlantFloorSectionModel> GetSectionById(int sectionId);
        Task<PlantFloorSectionModel> UpdateSectionAsync(int sectionId, string name);
        Task<bool> DeleteSectionByCodeAsync(int sectionId);
    }
}