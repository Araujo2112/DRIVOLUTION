using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class WorkstationRepository : IWorkstationRepository
{
    private readonly ApplicationDbContext _context;
    public WorkstationRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<WorkstationModel>> GetAll() =>
        await _context.Workstations
            .Include(w => w.ProductionLine)
            .Include(w => w.ManufacturingPhase)
            .ToListAsync();

    public async Task<IEnumerable<WorkstationModel>> GetByProductionLine(int productionLineId) =>
        await _context.Workstations
            .Include(w => w.ManufacturingPhase)
            .Where(w => w.ProductionLineId == productionLineId)
            .ToListAsync();

    public async Task<WorkstationModel?> GetById(int id) =>
        await _context.Workstations
            .Include(w => w.ProductionLine)
            .Include(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(w => w.Id == id);

    public async Task<WorkstationModel> Create(WorkstationModel entity)
    {
        _context.Workstations.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task Update(WorkstationModel entity)
    {
        _context.Workstations.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var entity = await _context.Workstations.FindAsync(id);
        if (entity != null) { _context.Workstations.Remove(entity); await _context.SaveChangesAsync(); }
    }

    public async Task<bool> Exists(int id) => await _context.Workstations.AnyAsync(w => w.Id == id);
}