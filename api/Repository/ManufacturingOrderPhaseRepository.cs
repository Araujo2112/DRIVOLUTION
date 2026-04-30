using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingOrderPhase;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;



public class ManufacturingOrderPhaseRepository : IManufacturingOrderPhaseRepository
{
    private readonly ApplicationDbContext _context;

    public ManufacturingOrderPhaseRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ManufacturingOrderPhaseModel>> GetAll()
    {
        return await _context.ManufacturingOrderPhases
            .Include(mop => mop.ManufacturingOrder)
            .Include(mop => mop.ManufacturingPhase)
            .ToListAsync();
    }

    public async Task<ManufacturingOrderPhaseModel> GetById(int id)
    {
        var manufacturingOrderPhase = await _context.ManufacturingOrderPhases
            .Include(mop => mop.ManufacturingOrder)
            .Include(mop => mop.ManufacturingPhase)
            .FirstOrDefaultAsync(mop => mop.Id == id);

        if (manufacturingOrderPhase == null)
            throw new KeyNotFoundException($"Manufacturing Order Phase with ID {id} not found");

        return manufacturingOrderPhase;
    }

    public async Task<ManufacturingOrderPhaseModel> Create(ManufacturingOrderPhaseModel manufacturingOrderPhase)
    {
        _context.ManufacturingOrderPhases.Add(manufacturingOrderPhase);
        await _context.SaveChangesAsync();
        return manufacturingOrderPhase;
    }

    public async Task Update(ManufacturingOrderPhaseModel manufacturingOrderPhase)
    {
        _context.Entry(manufacturingOrderPhase).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var manufacturingOrderPhase = await GetById(id);
        _context.ManufacturingOrderPhases.Remove(manufacturingOrderPhase);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.ManufacturingOrderPhases.AnyAsync(mop => mop.Id == id);
    }
}