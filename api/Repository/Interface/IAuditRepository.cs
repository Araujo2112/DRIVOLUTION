using Drivolution.Models;

namespace Drivolution.Repository.Interface;

public interface IAuditRepository
{
    Task<IEnumerable<AuditLogModel>> GetAllAsync();
    Task<IEnumerable<AuditLogModel>> GetByEntityAsync(string entity);
    Task<IEnumerable<AuditLogModel>> GetByUserAsync(int userId);
}