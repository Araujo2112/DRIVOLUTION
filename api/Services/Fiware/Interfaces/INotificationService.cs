using System.Text.Json;

namespace ApiTexPact.Services.Interface;

public interface INotificationService
{
    Task<bool> ProcessNotification(JsonElement notification);
}
