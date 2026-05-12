using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class ManufacturingPhaseRepository : IManufacturingPhaseRepository
{
    private readonly ApplicationDbContext _context;
    public ManufacturingPhaseRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<ManufacturingPhaseModel>> GetAll() => await _context.ManufacturingPhases.ToListAsync();
    public async Task<ManufacturingPhaseModel?> GetById(int id) => await _context.ManufacturingPhases.FindAsync(id);
    public async Task<ManufacturingPhaseModel> Create(ManufacturingPhaseModel entity)
    {
        _context.ManufacturingPhases.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(ManufacturingPhaseModel entity)
    {
        _context.ManufacturingPhases.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.ManufacturingPhases.FindAsync(id);
        if (entity != null) { _context.ManufacturingPhases.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.ManufacturingPhases.AnyAsync(mp => mp.Id == id);
}
