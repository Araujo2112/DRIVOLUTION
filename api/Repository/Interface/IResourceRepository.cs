using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IResourceRepository
{
    Task<IEnumerable<ResourceModel>> GetAll();
    Task<ResourceModel?> GetById(int id);
    Task<ResourceModel> Create(ResourceModel entity);
    Task Update(ResourceModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
