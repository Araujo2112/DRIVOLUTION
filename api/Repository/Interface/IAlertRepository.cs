using Drivolution.DTO;
using Drivolution.Models;

namespace Drivolution.Repository.Interface;

public interface IAlertRepository
{
    Task<PagedResultDTO<AlertModel>> GetPagedAsync(int page, int pageSize, string? type, string? status);
    Task<IEnumerable<AlertModel>> GetAllAsync();
    Task<IEnumerable<AlertModel>> GetOpenAsync();
    Task<bool> ExistsOpenForPhaseAsync(int productPhaseId, string type);
    Task<AlertModel?> GetByIdAsync(int id);
    Task<AlertModel> CreateAsync(AlertModel alert);
    Task<AlertModel> UpdateAsync(AlertModel alert);
    Task<IEnumerable<AlertModel>> GetOpenByTypeAsync(string type);
    Task<IEnumerable<AlertModel>> GetPendingByProductAndTypeAsync(int productId, string type);
}