using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface ICarModelRepository
{
    Task<IEnumerable<CarModelModel>> GetAll();
    Task<CarModelModel?> GetById(int id);
    Task<CarModelModel?> GetByIdWithPhaseSequence(int id);
    Task<IEnumerable<ConfigModel>> GetConfigs(int modelId);
    Task<CarModelModel> Create(CarModelModel entity);
    Task Update(CarModelModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
