using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Material;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class MaterialRepository : IMaterialRepository
{
    private readonly ApplicationDbContext _context;
    public MaterialRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<MaterialModel>> GetAll() => await _context.Materials.ToListAsync();
    public async Task<MaterialModel?> GetById(int id) => await _context.Materials.FindAsync(id);
    public async Task<MaterialModel> Create(MaterialModel entity)
    {
        _context.Materials.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(MaterialModel entity)
    {
        _context.Materials.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.Materials.FindAsync(id);
        if (entity != null) { _context.Materials.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.Materials.AnyAsync(m => m.Id == id);
}
