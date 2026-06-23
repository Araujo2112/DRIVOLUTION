using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace Drivolution.Repository;
public class SupportRepository : ISupportRepository
{
    private readonly ApplicationDbContext _context;
    public SupportRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<SupportModel>> GetAll() =>
        await _context.Supports.Include(s => s.ProductionLine).ToListAsync();
    public async Task<SupportModel?> GetById(int id) =>
        await _context.Supports.Include(s => s.ProductionLine).FirstOrDefaultAsync(s => s.Id == id);
    public async Task<SupportModel?> GetByRfidTag(string rfidTag) =>
        await _context.Supports.FirstOrDefaultAsync(s => s.RfidTag == rfidTag);
    public async Task<SupportModel> Create(SupportModel entity)
    {
        _context.Supports.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(SupportModel entity)
    {
        _context.Supports.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.Supports.FindAsync(id);
        if (entity != null) { _context.Supports.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.Supports.AnyAsync(s => s.Id == id);
}
