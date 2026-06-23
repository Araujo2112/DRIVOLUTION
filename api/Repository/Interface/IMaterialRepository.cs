using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IMaterialRepository
{
    Task<IEnumerable<MaterialModel>> GetAll();
    Task<MaterialModel?> GetById(int id);
    Task<MaterialModel> Create(MaterialModel entity);
    Task Update(MaterialModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
