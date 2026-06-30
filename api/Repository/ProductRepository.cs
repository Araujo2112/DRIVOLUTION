using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    public ProductRepository(ApplicationDbContext context) => _context = context;

    public async Task<PagedResultDTO<ProductModel>> GetPaged(
        int page, int pageSize, string? search, int? modelId, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _context.Products
            .Include(p => p.CarModel)
            .Include(p => p.ManufacturingOrder)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p =>
                (p.SerialNumber != null && p.SerialNumber.ToLower().Contains(s)) ||
                (p.LotNumber != null && p.LotNumber.ToLower().Contains(s)));
        }

        if (modelId.HasValue)
            query = query.Where(p => p.ModelId == modelId.Value);

        if (dateFrom.HasValue)
            query = query.Where(p => p.ProductionDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(p => p.ProductionDate <= dateTo.Value.AddDays(1));

        var total = await query.CountAsync();

        var data = await query
            .OrderByDescending(p => p.ProductionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDTO<ProductModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ProductModel?> GetById(int id) =>
        await _context.Products.Include(p => p.CarModel).Include(p => p.ManufacturingOrder)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<IEnumerable<ProductModel>> GetByManufacturingOrder(int orderId) =>
        await _context.Products.Where(p => p.ManufacturingOrderId == orderId)
            .Include(p => p.CarModel).ToListAsync();

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

    public async Task<bool> Exists(int id) =>
        await _context.Products.AnyAsync(p => p.Id == id);
}