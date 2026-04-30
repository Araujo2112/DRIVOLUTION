using ApiTexPact.Models;

namespace ApiTexPact.Services.Interface.Containers;

public interface IContainerService
{
    Task<List<ContainerModel>> GetAllContainersAsync();
    Task<ContainerModel> GetContainerByCodeAsync(string containerCode);
    Task<ContainerModel> GetContainerId(int containerId);
    Task<ContainerModel> CreateContainerAsync(ContainerModel container);
    Task<ContainerModel> UpdateContainerAsync(int containerId, string containerName, float containerVolume, bool activate);
    Task<bool> DeleteContainerByCodeAsync(int containerId);
}