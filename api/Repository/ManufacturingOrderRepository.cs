using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingOrder;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class ManufacturingOrderRepository : IManufacturingOrderRepository
{
    private readonly ApplicationDbContext _context;
    public ManufacturingOrderRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<ManufacturingOrderModel>> GetAll() =>
        await _context.ManufacturingOrders.Include(mo => mo.ClientOrder).ToListAsync();
    public async Task<ManufacturingOrderModel?> GetById(int id) =>
        await _context.ManufacturingOrders.Include(mo => mo.ClientOrder).FirstOrDefaultAsync(mo => mo.Id == id);
    public async Task<ManufacturingOrderModel?> GetByIdWithDetails(int id) =>
        await _context.ManufacturingOrders
            .Include(mo => mo.ClientOrder)
            .Include(mo => mo.Products).ThenInclude(p => p.CarModel)
            .Include(mo => mo.Products).ThenInclude(p => p.ProductPhases).ThenInclude(pp => pp.ManufacturingPhase)
            .FirstOrDefaultAsync(mo => mo.Id == id);
    public async Task<IEnumerable<ManufacturingOrderModel>> GetByStatus(string status) =>
        await _context.ManufacturingOrders.Where(mo => mo.Status == status).Include(mo => mo.ClientOrder).ToListAsync();
    public async Task<ManufacturingOrderModel> Create(ManufacturingOrderModel entity)
    {
        _context.ManufacturingOrders.Add(entity);
        await _context.SaveChangesAsync();
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
    public async Task<bool> Exists(int id) => await _context.ManufacturingOrders.AnyAsync(mo => mo.Id == id);
}
