using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class CarModelRepository : ICarModelRepository
{
    private readonly ApplicationDbContext _context;
    public CarModelRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<CarModelModel>> GetAll() => await _context.CarModels.ToListAsync();
    public async Task<CarModelModel?> GetById(int id) => await _context.CarModels.FindAsync(id);
    public async Task<CarModelModel?> GetByIdWithPhaseSequence(int id) =>
        await _context.CarModels.Include(m => m.PhaseSequences).ThenInclude(ps => ps.ManufacturingPhase).FirstOrDefaultAsync(m => m.Id == id);
    public async Task<IEnumerable<ConfigModel>> GetConfigs(int modelId) =>
        await _context.Configs.Where(c => c.ModelId == modelId).ToListAsync();
    public async Task<CarModelModel> Create(CarModelModel entity)
    {
        _context.CarModels.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(CarModelModel entity)
    {
        _context.CarModels.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.CarModels.FindAsync(id);
        if (entity != null) { _context.CarModels.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.CarModels.AnyAsync(m => m.Id == id);
}
