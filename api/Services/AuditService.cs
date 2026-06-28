using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task LogAsync(int userId, string userName, string action, string entity, int entityId, string entityLabel)
    {
        // A auditoria nunca deve impedir a operação de negócio que a chamou.
        // Se o registo falhar (ex: nova entidade esquecida no CHECK constraint da BD),
        // a falha fica só nos logs do servidor — não chega ao utilizador como um erro 500.
        var log = new AuditLogModel
        {
            UserId      = userId,
            UserName    = userName,
            Action      = action,
            Entity      = entity,
            EntityId    = entityId,
            EntityLabel = entityLabel,
            CreatedAt   = DateTime.UtcNow,
        };

        try
        {
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Falha ao registar audit log: action={Action}, entity={Entity}, entityId={EntityId}. " +
                "A operação principal NÃO foi afetada por este erro.",
                action, entity, entityId);

            // Remove a entidade falhada do ChangeTracker — caso contrário, qualquer
            // SaveChangesAsync() posterior no mesmo request (mesmo DbContext, scoped)
            // tentaria gravá-la outra vez e falharia de novo com o mesmo erro.
            _context.Entry(log).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
        }
    }
}