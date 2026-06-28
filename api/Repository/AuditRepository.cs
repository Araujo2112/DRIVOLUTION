using Drivolution.Data;
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