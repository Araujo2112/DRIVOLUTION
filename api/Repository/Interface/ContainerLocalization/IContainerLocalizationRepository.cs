using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface
{
    public interface IContainerLocalizationRepository
    {
        Task<ContainerLocalizationModel> CreateContainerLocalizationAsync(ContainerLocalizationModel localization);
        Task<List<ContainerLocalizationModel>> GetAllContainerLocalizationsAsync();
        Task<ContainerLocalizationModel> GetContainerLocalizationByIdAsync(int id); 
        
        Task<ContainerLocalizationModel> GetContainerLocalizationByContainerAndSectionAsync(int containerId, int sectionId);
        Task<ContainerLocalizationModel> GetLastContainerLocalizationAsync(int containerId);
        Task<ContainerLocalizationModel> UpdateContainerLocalizationAsync(int id, int containerId, int sectionId, DateTime datetime);
        Task<bool> DeleteContainerLocalizationByIdAsync(int id); 
    }
}