using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingPhase;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;



public class ManufacturingPhaseRepository : IManufacturingPhaseRepository
{
    private readonly ApplicationDbContext _context;

    public ManufacturingPhaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ManufacturingPhaseModel>> GetAll()
    {
        return await _context.ManufacturingPhases
                             .Include(mp => mp.PlantFloorSection)
                             .ToListAsync();
    }

    public async Task<ManufacturingPhaseModel> GetById(int id)
    {
        var manufacturingPhase = await _context.ManufacturingPhases
                                                .Include(mp => mp.PlantFloorSection)
                                                .FirstOrDefaultAsync(mp => mp.Id == id);

        if (manufacturingPhase == null)
        {
            throw new KeyNotFoundException($"Manufacturing Phase with ID {id} not found");
        }

        return manufacturingPhase;
    }

    public async Task<ManufacturingPhaseModel> Create(ManufacturingPhaseModel manufacturingPhase)
    {
        _context.ManufacturingPhases.Add(manufacturingPhase);
        await _context.SaveChangesAsync();
        return manufacturingPhase;
    }

    public async Task Update(ManufacturingPhaseModel manufacturingPhase)
    {
        _context.Entry(manufacturingPhase).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var manufacturingPhase = await GetById(id);
        _context.ManufacturingPhases.Remove(manufacturingPhase);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.ManufacturingPhases.AnyAsync(mp => mp.Id == id);
    }
}
