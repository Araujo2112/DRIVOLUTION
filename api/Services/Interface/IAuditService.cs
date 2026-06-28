namespace Drivolution.Services.Interface;

public interface IAuditService
{
    Task LogAsync(int userId, string userName, string action, string entity, int entityId, string entityLabel);
}