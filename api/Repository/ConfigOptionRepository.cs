using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class ConfigOptionRepository : IConfigOptionRepository
{
    private readonly ApplicationDbContext _context;
    public ConfigOptionRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ConfigOptionModel>> GetAll() => 
        await _context.Set<ConfigOptionModel>().ToListAsync();

    public async Task<ConfigOptionModel?> GetById(int id) => 
        await _context.Set<ConfigOptionModel>().FindAsync(id);

    public async Task<IEnumerable<ConfigOptionModel>> GetByConfigId(int configId) =>
        await _context.Set<ConfigOptionModel>().Where(o => o.ConfigId == configId).ToListAsync();

    public async Task<ConfigOptionModel?> GetDefaultByConfigId(int configId) =>
        await _context.Set<ConfigOptionModel>().FirstOrDefaultAsync(o => o.ConfigId == configId && o.IsDefault);

    public async Task<ConfigOptionModel> Create(ConfigOptionModel entity)
    {
        _context.Set<ConfigOptionModel>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task Update(ConfigOptionModel entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var entity = await GetById(id);
        if (entity != null)
        {
            _context.Set<ConfigOptionModel>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> Exists(int id) => 
        await _context.Set<ConfigOptionModel>().AnyAsync(e => e.Id == id);

    public async Task<IEnumerable<ConfigOptionModel>> GetDefaultsByConfigId(int configId) =>
        await _context.Set<ConfigOptionModel>().Where(o => o.ConfigId == configId && o.IsDefault).ToListAsync();
}