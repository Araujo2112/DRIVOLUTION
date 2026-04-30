using ApiTexPact.Models;

namespace ApiTexPact.Services.Interface.Containers
{
    public interface IContainerLocalizationService
    {
        Task<List<ContainerLocalizationModel>> GetAllContainerLocalizationsAsync();
        Task<ContainerLocalizationModel> GetContainerLocalizationByIdAsync(int id); 
        Task<ContainerLocalizationModel> CreateContainerLocalizationAsync(ContainerLocalizationModel localization);
        Task<ContainerLocalizationModel> UpdateContainerLocalizationAsync(int id, int containerId, int sectionId, DateTime datetime); 
        Task<bool> DeleteContainerLocalizationByIdAsync(int id); 
    }
}