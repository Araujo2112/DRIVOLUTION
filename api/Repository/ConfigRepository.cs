using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class ConfigRepository : IConfigRepository
{
    private readonly ApplicationDbContext _context;
    public ConfigRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ConfigModel>> GetAll() => 
        await _context.Set<ConfigModel>().ToListAsync();

    public async Task<ConfigModel?> GetById(int id) => 
        await _context.Set<ConfigModel>().FindAsync(id);

    public async Task<IEnumerable<ConfigModel>> GetByModelId(int modelId) =>
        await _context.Set<ConfigModel>().Where(c => c.ModelId == modelId).ToListAsync();

    public async Task<ConfigModel> Create(ConfigModel entity)
    {
        _context.Set<ConfigModel>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task Update(ConfigModel entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var entity = await GetById(id);
        if (entity != null)
        {
            _context.Set<ConfigModel>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> Exists(int id) => 
        await _context.Set<ConfigModel>().AnyAsync(e => e.Id == id);
}