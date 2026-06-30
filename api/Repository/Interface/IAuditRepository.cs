using Drivolution.DTO;
using Drivolution.Models;

namespace Drivolution.Repository.Interface;

public interface IAuditRepository
{
    Task<PagedResultDTO<AuditLogModel>> GetPagedAsync(int page, int pageSize, string? entity, int? userId, string? action);
    Task<IEnumerable<(int UserId, string UserName)>> GetDistinctUsersAsync();
    Task<IEnumerable<AuditLogModel>> GetAllAsync();
    Task<IEnumerable<AuditLogModel>> GetByEntityAsync(string entity);
    Task<IEnumerable<AuditLogModel>> GetByUserAsync(int userId);
}