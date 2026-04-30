using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Product;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductModel>> GetAll()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<ProductModel> GetById(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (product == null) throw new KeyNotFoundException($"Product with ID {id} not found");

        return product;
    }

    public async Task<ProductModel> Create(ProductModel product)
    {
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return product;
    }

    public async Task Update(ProductModel product)
    {
        _context.Entry(product).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var product = await GetById(id);
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }
}