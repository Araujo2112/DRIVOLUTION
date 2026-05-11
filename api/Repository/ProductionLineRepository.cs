using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ProductionLine;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class ProductionLineRepository : IProductionLineRepository
{
    private readonly ApplicationDbContext _context;
    public ProductionLineRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<ProductionLineModel>> GetAll() =>
        await _context.ProductionLines.Include(p => p.Workstations).ToListAsync();
    public async Task<ProductionLineModel?> GetById(int id) =>
        await _context.ProductionLines.Include(p => p.Workstations).FirstOrDefaultAsync(p => p.Id == id);
    public async Task<ProductionLineModel> Create(ProductionLineModel entity)
    {
        _context.ProductionLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(ProductionLineModel entity)
    {
        _context.ProductionLines.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.ProductionLines.FindAsync(id);
        if (entity != null) { _context.ProductionLines.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.ProductionLines.AnyAsync(p => p.Id == id);
}
