using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.SupportedProduct;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class SupportedProductRepository : ISupportedProductRepository
{
    private readonly ApplicationDbContext _context;
    public SupportedProductRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<SupportedProductModel>> GetBySupport(int supportId) =>
        await _context.SupportedProducts.Where(sp => sp.SupportId == supportId).Include(sp => sp.Product).ToListAsync();
    public async Task<SupportedProductModel?> GetCurrentBySupport(int supportId) =>
        await _context.SupportedProducts.Where(sp => sp.SupportId == supportId && sp.DatetimeEnd == null).Include(sp => sp.Product).FirstOrDefaultAsync();
    public async Task<SupportedProductModel> Create(SupportedProductModel entity)
    {
        _context.SupportedProducts.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(SupportedProductModel entity)
    {
        _context.SupportedProducts.Update(entity);
        await _context.SaveChangesAsync();
    }
}
