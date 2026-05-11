using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.Config;

public interface IConfigOptionRepository
{
    Task<IEnumerable<ConfigOptionModel>> GetAll();
    Task<ConfigOptionModel?> GetById(int id);
    Task<IEnumerable<ConfigOptionModel>> GetByConfigId(int configId);
    Task<ConfigOptionModel?> GetDefaultByConfigId(int configId);
    Task<ConfigOptionModel> Create(ConfigOptionModel entity);
    Task Update(ConfigOptionModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}