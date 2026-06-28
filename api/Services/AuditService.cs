using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int userId, string userName, string action, string entity, int entityId, string entityLabel)
    {
        _context.AuditLogs.Add(new AuditLogModel
        {
            UserId      = userId,
            UserName    = userName,
            Action      = action,
            Entity      = entity,
            EntityId    = entityId,
            EntityLabel = entityLabel,
            CreatedAt   = DateTime.UtcNow,
        });

        await _context.SaveChangesAsync();
    }
}