using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável por registar ações de auditoria na base de dados
public class AuditService : IAuditService
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // Logger utilizado para registar erros caso a auditoria falhe
    private readonly ILogger<AuditService> _logger;

    // O ASP.NET injeta automaticamente as dependências necessárias
    public AuditService(ApplicationDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Regista uma ação de auditoria
    public async Task LogAsync(
        int userId,
        string userName,
        string action,
        string entity,
        int entityId,
        string entityLabel)
    {
        // A auditoria nunca deve impedir a operação de negócio que a chamou.
        // Se o registo falhar (ex: nova entidade esquecida no CHECK constraint da BD),
        // a falha fica só nos logs do servidor — não chega ao utilizador como um erro 500.

        // Cria o registo de auditoria
        var log = new AuditLogModel
        {
            UserId = userId,
            UserName = userName,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            EntityLabel = entityLabel,
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            // Adiciona o registo à base de dados
            _context.AuditLogs.Add(log);

            // Guarda as alterações
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Regista o erro nos logs do servidor
            _logger.LogError(
                ex,
                "Falha ao registar audit log: action={Action}, entity={Entity}, entityId={EntityId}. " +
                "A operação principal NÃO foi afetada por este erro.",
                action,
                entity,
                entityId);

            // Remove a entidade falhada do ChangeTracker — caso contrário, qualquer
            // SaveChangesAsync() posterior no mesmo request (mesmo DbContext, scoped)
            // tentaria gravá-la outra vez e falharia de novo com o mesmo erro.
            _context.Entry(log).State =
                Microsoft.EntityFrameworkCore.EntityState.Detached;
        }
    }
}