using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace Drivolution.Repository;
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

    public async Task<ProductConfigModel?> GetByProductAndOption(int productId, int configOptionId) =>
        await _context.ProductConfigs.FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.ConfigOptionId == configOptionId);

    public async Task<IEnumerable<ProductConfigModel>> GetByProductAndConfig(int productId, int configId) =>
        await _context.ProductConfigs
            .Include(pc => pc.ConfigOption)
            .Where(pc => pc.ProductId == productId && pc.ConfigOption.ConfigId == configId)
            .ToListAsync();
}
