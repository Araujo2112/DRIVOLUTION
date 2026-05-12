using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface;

public interface IConfigRepository
{
    Task<IEnumerable<ConfigModel>> GetAll();
    Task<ConfigModel?> GetById(int id);
    Task<IEnumerable<ConfigModel>> GetByModelId(int modelId);
    Task<ConfigModel> Create(ConfigModel entity);
    Task Update(ConfigModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}