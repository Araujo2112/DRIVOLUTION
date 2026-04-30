using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ProductLot;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class ProductLotRepository : IProductLotRepository
{
    private readonly ApplicationDbContext _context;

    public ProductLotRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductLotModel>> GetAll()
    {
        return await _context.ProductLots
            .Include(pl => pl.Product)
            .ToListAsync();
    }

    public async Task<ProductLotModel> GetById(int id)
    {
        var productLot = await _context.ProductLots
            .Include(pl => pl.Product)
            .FirstOrDefaultAsync(pl => pl.Id == id);

        if (productLot == null) throw new KeyNotFoundException($"Product Lot with ID {id} not found");

        return productLot;
    }

    public async Task<ProductLotModel> Create(ProductLotModel productLot)
    {
        _context.ProductLots.Add(productLot);
        await _context.SaveChangesAsync();
        return productLot;
    }

    public async Task Update(ProductLotModel productLot)
    {
        _context.Entry(productLot).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var productLot = await GetById(id);
        _context.ProductLots.Remove(productLot);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.ProductLots.AnyAsync(pl => pl.Id == id);
    }
}