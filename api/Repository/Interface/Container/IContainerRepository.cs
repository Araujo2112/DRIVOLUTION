using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface;

public interface IContainerRepository
{
    Task<List<ContainerModel>> GetAllContainersAsync();
    Task<ContainerModel?> GetContainerByCodeAsync(string ContainerCode);
    
    Task<ContainerModel?> GetContainerById(int ContainerId);
    
    Task AddContainerAsync(ContainerModel container);
    Task UpdateContainerAsync(ContainerModel container);
    Task DeleteContainerAsync(int ContainerId);
    
    
}