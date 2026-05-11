using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ProductConfig;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class ProductConfigRepository : IProductConfigRepository
{
    private readonly ApplicationDbContext _context;
    public ProductConfigRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<ProductConfigModel>> GetByProduct(int productId) =>
        await _context.ProductConfigs.Where(pc => pc.ProductId == productId).Include(pc => pc.ConfigOption).ToListAsync();
    public async Task<ProductConfigModel> Create(ProductConfigModel entity)
    {
        _context.ProductConfigs.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(ProductConfigModel entity)
    {
        _context.ProductConfigs.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.ProductConfigs.FindAsync(id);
        if (entity != null) { _context.ProductConfigs.Remove(entity); await _context.SaveChangesAsync(); }
    }
}
