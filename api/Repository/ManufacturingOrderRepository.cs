using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class ManufacturingOrderRepository : IManufacturingOrderRepository
{
    private readonly ApplicationDbContext _context;
    public ManufacturingOrderRepository(ApplicationDbContext context) => _context = context;

    public async Task<PagedResultDTO<ManufacturingOrderModel>> GetPaged(
        int page, int pageSize, string? search, string? status, DateTime? dateFrom, DateTime? dateTo)
    {
        var query = _context.ManufacturingOrders
            .Include(mo => mo.ClientOrder)
                .ThenInclude(c => c.AppUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && status != "all")
            query = query.Where(mo => mo.Status == status);

        if (dateFrom.HasValue)
            query = query.Where(mo => mo.StartDate >= dateFrom.Value);

        if (dateTo.HasValue)
            query = query.Where(mo => mo.StartDate <= dateTo.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(mo =>
                mo.ManufacturingOrderNumber.ToLower().Contains(s) ||
                (mo.ClientOrder.AppUser != null && mo.ClientOrder.AppUser.Name.ToLower().Contains(s)));
        }

        var total = await query.CountAsync();

        var data = await query
            .OrderByDescending(mo => mo.StartDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDTO<ManufacturingOrderModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<ManufacturingOrderModel?> GetById(int id) =>
        await _context.ManufacturingOrders
            .Include(mo => mo.ClientOrder).ThenInclude(c => c.AppUser)
            .FirstOrDefaultAsync(mo => mo.Id == id);

    public async Task<ManufacturingOrderModel?> GetByIdWithDetails(int id) =>
        await _context.ManufacturingOrders
            .Include(mo => mo.ClientOrder).ThenInclude(c => c.AppUser)
            .Include(mo => mo.Products).ThenInclude(p => p.CarModel)
            .Include(mo => mo.Products).ThenInclude(p => p.ProductConfigs)
                .ThenInclude(pc => pc.ConfigOption).ThenInclude(co => co.Config)
            .Include(mo => mo.Products).ThenInclude(p => p.ProductPhases)
                .ThenInclude(pp => pp.ManufacturingPhase)
            .FirstOrDefaultAsync(mo => mo.Id == id);

    public async Task<ManufacturingOrderModel> Create(ManufacturingOrderModel entity)
    {
        _context.ManufacturingOrders.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(mo => mo.ClientOrder).LoadAsync();
        if (entity.ClientOrder != null)
            await _context.Entry(entity.ClientOrder).Reference(c => c.AppUser).LoadAsync();
        return entity;
    }

    public async Task Update(ManufacturingOrderModel entity)
    {
        _context.ManufacturingOrders.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var entity = await _context.ManufacturingOrders.FindAsync(id);
        if (entity != null) { _context.ManufacturingOrders.Remove(entity); await _context.SaveChangesAsync(); }
    }

    public async Task<bool> Exists(int id) =>
        await _context.ManufacturingOrders.AnyAsync(mo => mo.Id == id);
}