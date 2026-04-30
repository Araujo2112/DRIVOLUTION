using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface;

public interface IPlantFloorSectionRepository
{
    Task<PlantFloorSectionModel> CreatePlantFloorSectionAsync(PlantFloorSectionModel section);
    Task<List<PlantFloorSectionModel>> GetAllPlantFloorSectionsAsync();
    Task<PlantFloorSectionModel> GetSectionByCodeAsync(string sectionCode);
    
    Task<PlantFloorSectionModel> GetSectionById(int sectionId);
    
    Task<bool> DeletePlantFloorSectionByCodeAsync(int sectionId);
    Task<PlantFloorSectionModel> UpdatePlantFloorSectionAsync(int sectionId, string name);
}