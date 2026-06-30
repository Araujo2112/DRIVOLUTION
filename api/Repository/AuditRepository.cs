using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class AuditRepository : IAuditRepository
{
    private readonly ApplicationDbContext _context;

    public AuditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDTO<AuditLogModel>> GetPagedAsync(int page, int pageSize, string? entity, int? userId, string? action)
    {
        var query = _context.AuditLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(entity))
            query = query.Where(a => a.Entity == entity);

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        var total = await query.CountAsync();

        var data = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDTO<AuditLogModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Lista de utilizadores distintos que já geraram logs — usada para popular
    // o dropdown de filtro sem depender da página atual carregada.
    public async Task<IEnumerable<(int UserId, string UserName)>> GetDistinctUsersAsync()
    {
        var users = await _context.AuditLogs
            .Select(a => new { a.UserId, a.UserName })
            .Distinct()
            .OrderBy(a => a.UserName)
            .ToListAsync();

        return users.Select(u => (u.UserId, u.UserName));
    }

    public async Task<IEnumerable<AuditLogModel>> GetAllAsync()
        => await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<AuditLogModel>> GetByEntityAsync(string entity)
        => await _context.AuditLogs
            .Where(a => a.Entity == entity)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<IEnumerable<AuditLogModel>> GetByUserAsync(int userId)
        => await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
}