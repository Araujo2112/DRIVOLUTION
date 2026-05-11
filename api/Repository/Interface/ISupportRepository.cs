using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface.Support;
public interface ISupportRepository
{
    Task<IEnumerable<SupportModel>> GetAll();
    Task<SupportModel?> GetById(int id);
    Task<SupportModel?> GetByRfidTag(string rfidTag);
    Task<SupportModel> Create(SupportModel entity);
    Task Update(SupportModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
