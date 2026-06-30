using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Microsoft.EntityFrameworkCore;
using Drivolution.Repository.Interface;

namespace Drivolution.Repository;

public class AlertRepository : IAlertRepository
{
    private readonly ApplicationDbContext _context;

    public AlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDTO<AlertModel>> GetPagedAsync(int page, int pageSize, string? type, string? status)
    {
        var query = _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(a => a.Type == type);

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        var total = await query.CountAsync();

        var data = await query
            .OrderByDescending(a => a.TriggeredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDTO<AlertModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<AlertModel>> GetAllAsync()
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

    public async Task<IEnumerable<AlertModel>> GetOpenAsync()
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .Where(a => a.Status == "open")
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

    public async Task<bool> ExistsOpenForPhaseAsync(int productPhaseId, string type)
        => await _context.Alerts
            .AnyAsync(a => a.ProductPhaseId == productPhaseId 
                        && a.Type == type 
                        && (a.Status == "open" || a.Status == "acknowledged"));

    public async Task<AlertModel?> GetByIdAsync(int id)
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .FirstOrDefaultAsync(a => a.Id == id);

    public async Task<AlertModel> CreateAsync(AlertModel alert)
    {
        _context.Alerts.Add(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task<AlertModel> UpdateAsync(AlertModel alert)
    {
        _context.Alerts.Update(alert);
        await _context.SaveChangesAsync();
        return alert;
    }

    public async Task<IEnumerable<AlertModel>> GetOpenByTypeAsync(string type)
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .Where(a => (a.Status == "open" || a.Status == "acknowledged") && a.Type == type)
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

    public async Task<IEnumerable<AlertModel>> GetPendingByProductAndTypeAsync(int productId, string type)
        => await _context.Alerts
            .Where(a => a.ProductId == productId 
                    && a.Type == type 
                    && (a.Status == "open" || a.Status == "acknowledged"))
            .ToListAsync();
}