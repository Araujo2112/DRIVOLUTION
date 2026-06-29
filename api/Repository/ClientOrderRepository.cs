using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace Drivolution.Repository;
public class ClientOrderRepository : IClientOrderRepository
{
    private readonly ApplicationDbContext _context;
    public ClientOrderRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<ClientOrderModel>> GetAll() =>
        await _context.ClientOrders.Include(c => c.AppUser).ToListAsync();

    public async Task<ClientOrderModel?> GetById(int id) =>
        await _context.ClientOrders.Include(c => c.AppUser).FirstOrDefaultAsync(c => c.Id == id);

    public async Task<ClientOrderModel> Create(ClientOrderModel entity)
    {
        _context.ClientOrders.Add(entity);
        await _context.SaveChangesAsync();
        await _context.Entry(entity).Reference(c => c.AppUser).LoadAsync();
        return entity;
    }
    public async Task Update(ClientOrderModel entity)
    {
        _context.ClientOrders.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.ClientOrders.FindAsync(id);
        if (entity != null) { _context.ClientOrders.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.ClientOrders.AnyAsync(c => c.Id == id);
}