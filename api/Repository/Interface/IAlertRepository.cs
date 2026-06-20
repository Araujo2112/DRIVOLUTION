using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface;

public interface IAlertRepository
{
    Task<IEnumerable<AlertModel>> GetAllAsync();
    Task<IEnumerable<AlertModel>> GetOpenAsync();
    Task<bool> ExistsOpenForPhaseAsync(int productPhaseId, string type);
    Task<AlertModel?> GetByIdAsync(int id);
    Task<AlertModel> CreateAsync(AlertModel alert);
    Task<AlertModel> UpdateAsync(AlertModel alert);
    Task<IEnumerable<AlertModel>> GetOpenByTypeAsync(string type);
    Task<IEnumerable<AlertModel>> GetPendingByProductAndTypeAsync(int productId, string type);}