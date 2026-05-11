using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Product;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    public ProductRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<ProductModel>> GetAll() =>
        await _context.Products.Include(p => p.CarModel).Include(p => p.ManufacturingOrder).ToListAsync();
    public async Task<ProductModel?> GetById(int id) =>
        await _context.Products.Include(p => p.CarModel).Include(p => p.ManufacturingOrder).FirstOrDefaultAsync(p => p.Id == id);
    public async Task<IEnumerable<ProductModel>> GetByManufacturingOrder(int orderId) =>
        await _context.Products.Where(p => p.ManufacturingOrderId == orderId).Include(p => p.CarModel).ToListAsync();
    public async Task<ProductModel> Create(ProductModel entity)
    {
        _context.Products.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(ProductModel entity)
    {
        _context.Products.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.Products.FindAsync(id);
        if (entity != null) { _context.Products.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.Products.AnyAsync(p => p.Id == id);
}
