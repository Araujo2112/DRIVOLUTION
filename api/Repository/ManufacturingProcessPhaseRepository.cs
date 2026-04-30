using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingProcessPhase;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class ManufacturingProcessPhaseRepository : IManufacturingProcessPhaseRepository
{
    private readonly ApplicationDbContext _context;

    public ManufacturingProcessPhaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ManufacturingProcessPhaseModel>> GetAll()
    {
        return await _context.ManufacturingProcessPhases
            .Include(mpp => mpp.ManufacturingProcess)
            .Include(mpp => mpp.ManufacturingPhase)
            .ToListAsync();
    }

    public async Task<ManufacturingProcessPhaseModel> GetById(int manufacturingProcessId, int manufacturingPhaseId)
    {
        var manufacturingProcessPhase = await _context.ManufacturingProcessPhases
            .Include(mpp => mpp.ManufacturingProcess)
            .Include(mpp => mpp.ManufacturingPhase)
            .FirstOrDefaultAsync(mpp => mpp.ManufacturingProcessId == manufacturingProcessId
                                        && mpp.ManufacturingPhaseId == manufacturingPhaseId);

        if (manufacturingProcessPhase == null)
            throw new KeyNotFoundException(
                $"Manufacturing Process Phase with ManufacturingProcessId {manufacturingProcessId} and ManufacturingPhaseId {manufacturingPhaseId} not found");

        return manufacturingProcessPhase;
    }

    public async Task<ManufacturingProcessPhaseModel> Create(ManufacturingProcessPhaseModel manufacturingProcessPhase)
    {
        var phaseExists = await _context.ManufacturingPhases.AnyAsync(p => p.Id == manufacturingProcessPhase.ManufacturingPhaseId);
        var processExists = await _context.ManufacturingProcesses.AnyAsync(p => p.Id == manufacturingProcessPhase.ManufacturingProcessId);

        if (!phaseExists)
            throw new Exception($"ManufacturingPhaseId {manufacturingProcessPhase.ManufacturingPhaseId} does not exist.");

        if (!processExists)
            throw new Exception($"ManufacturingProcessId {manufacturingProcessPhase.ManufacturingProcessId} does not exist.");

        var alreadyExists = await _context.ManufacturingProcessPhases.AnyAsync(mpp =>
            mpp.ManufacturingProcessId == manufacturingProcessPhase.ManufacturingProcessId &&
            mpp.ManufacturingPhaseId == manufacturingProcessPhase.ManufacturingPhaseId);

        if (alreadyExists)
            throw new Exception("This ManufacturingProcessPhase relation already exists.");

        _context.ManufacturingProcessPhases.Add(manufacturingProcessPhase);
        await _context.SaveChangesAsync();
        return manufacturingProcessPhase;
    }

    public async Task Update(ManufacturingProcessPhaseModel manufacturingProcessPhase)
    {
        _context.Entry(manufacturingProcessPhase).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int manufacturingProcessId, int manufacturingPhaseId)
    {
        var manufacturingProcessPhase = await GetById(manufacturingProcessId, manufacturingPhaseId);
        _context.ManufacturingProcessPhases.Remove(manufacturingProcessPhase);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int manufacturingProcessId, int manufacturingPhaseId)
    {
        return await _context.ManufacturingProcessPhases.AnyAsync(mpp =>
            mpp.ManufacturingProcessId == manufacturingProcessId && mpp.ManufacturingPhaseId == manufacturingPhaseId);
    }
}