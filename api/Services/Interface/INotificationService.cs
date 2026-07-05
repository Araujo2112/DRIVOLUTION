using Drivolution.DTO.Client;

namespace Drivolution.Services.Interface;

public interface INotificationService
{
    Task CreateAsync(int appUserId, string type, string message, int? clientOrderId = null, int? productId = null);
    Task<bool> ExistsAsync(string type, int clientOrderId);
    Task<IEnumerable<NotificationDTO>> GetForUserAsync(int appUserId, int limit = 20);
    Task<int> CountUnreadAsync(int appUserId);
    Task<bool> MarkReadAsync(int id, int appUserId);
    Task MarkAllReadAsync(int appUserId);
    Task DeleteAllAsync(int appUserId);
}