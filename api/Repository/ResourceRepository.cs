using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class ResourceRepository : IResourceRepository
{
    private readonly ApplicationDbContext _context;
    public ResourceRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<ResourceModel>> GetAll() => await _context.Resources.ToListAsync();
    public async Task<ResourceModel?> GetById(int id) => await _context.Resources.FindAsync(id);
    public async Task<ResourceModel> Create(ResourceModel entity)
    {
        _context.Resources.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(ResourceModel entity)
    {
        _context.Resources.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.Resources.FindAsync(id);
        if (entity != null) { _context.Resources.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.Resources.AnyAsync(r => r.Id == id);
}
