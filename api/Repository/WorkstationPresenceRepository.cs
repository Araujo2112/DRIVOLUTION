using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class WorkstationPresenceRepository : IWorkstationPresenceRepository
{
    private readonly ApplicationDbContext _db;

    public WorkstationPresenceRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<WorkstationPresenceModel?> GetById(int id)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<WorkstationPresenceModel>> GetByWorkstation(int workstationId)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .Where(p => p.WorkstationId == workstationId)
            .OrderByDescending(p => p.CheckedInAt)
            .ToListAsync();

    public async Task<IEnumerable<WorkstationPresenceModel>> GetByUser(int appUserId)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .Where(p => p.AppUserId == appUserId)
            .OrderByDescending(p => p.CheckedInAt)
            .ToListAsync();

    public async Task<WorkstationPresenceModel?> GetActiveByUserAndWorkstation(int appUserId, int workstationId)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(p =>
                p.AppUserId == appUserId &&
                p.WorkstationId == workstationId &&
                p.CheckedOutAt == null);

    public async Task<WorkstationPresenceModel> Create(WorkstationPresenceModel entity)
    {
        _db.WorkstationPresences.Add(entity);
        await _db.SaveChangesAsync();
        return (await GetById(entity.Id))!;
    }

    public async Task Update(WorkstationPresenceModel entity)
    {
        _db.WorkstationPresences.Update(entity);
        await _db.SaveChangesAsync();
    }
}