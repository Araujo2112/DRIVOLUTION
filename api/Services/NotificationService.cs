using Drivolution.Data;
using Drivolution.DTO.Client;
using Drivolution.Models;
using Drivolution.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Services;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
    {
        _context = context;
        _logger  = logger;
    }

    public async Task CreateAsync(int appUserId, string type, string message, int? clientOrderId = null, int? productId = null)
    {
        var notification = new NotificationModel
        {
            AppUserId     = appUserId,
            Type          = type,
            Message       = message,
            ClientOrderId = clientOrderId,
            ProductId     = productId,
            IsRead        = false,
            CreatedAt     = DateTime.UtcNow,
        };

        try
        {
            _context.Set<NotificationModel>().Add(notification);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Falha ao criar notificação: type={Type}, appUserId={AppUserId}. " +
                "A operação principal NÃO foi afetada por este erro.",
                type, appUserId);

            _context.Entry(notification).State = EntityState.Detached;
        }
    }

    public async Task<bool> ExistsAsync(string type, int clientOrderId)
    {
        return await _context.Set<NotificationModel>()
            .AnyAsync(n => n.Type == type && n.ClientOrderId == clientOrderId);
    }

    public async Task<IEnumerable<NotificationDTO>> GetForUserAsync(int appUserId, int limit = 20)
    {
        return await _context.Set<NotificationModel>()
            .Where(n => n.AppUserId == appUserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .Select(n => new NotificationDTO
            {
                Id            = n.Id,
                Type          = n.Type,
                Message       = n.Message,
                ClientOrderId = n.ClientOrderId,
                ProductId     = n.ProductId,
                IsRead        = n.IsRead,
                CreatedAt     = n.CreatedAt,
            })
            .ToListAsync();
    }

    public async Task<int> CountUnreadAsync(int appUserId)
    {
        return await _context.Set<NotificationModel>()
            .CountAsync(n => n.AppUserId == appUserId && !n.IsRead);
    }

    public async Task<bool> MarkReadAsync(int id, int appUserId)
    {
        var notification = await _context.Set<NotificationModel>()
            .FirstOrDefaultAsync(n => n.Id == id && n.AppUserId == appUserId);

        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task MarkAllReadAsync(int appUserId)
    {
        await _context.Set<NotificationModel>()
            .Where(n => n.AppUserId == appUserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
    }

    public async Task DeleteAllAsync(int appUserId)
    {
        await _context.Set<NotificationModel>()
            .Where(n => n.AppUserId == appUserId)
            .ExecuteDeleteAsync();
    }
}