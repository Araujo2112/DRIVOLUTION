using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingOrderHistory;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;



public class ManufacturingOrderHistoryRepository : IManufacturingOrderHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public ManufacturingOrderHistoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ManufacturingOrderHistoryModel>> GetAll()
    {
        return await _context.ManufacturingOrderHistories
            .Include(m => m.ManufacturingOrder)
            .Include(m => m.PlantFloorSection)
            .ToListAsync();
    }

    public async Task<ManufacturingOrderHistoryModel?> GetById(int manufacturingOrderId, int plantFloorSectionId)
    {
        return await _context.ManufacturingOrderHistories
            .Include(h => h.PlantFloorSection)
            .Include(h => h.ManufacturingOrder)
            .FirstOrDefaultAsync(h =>
                h.ManufacturingOrderId == manufacturingOrderId &&
                h.PlantFloorSectionId == plantFloorSectionId);
    }


    public async Task<ManufacturingOrderHistoryModel> Create(ManufacturingOrderHistoryModel history)
    {
        _context.ManufacturingOrderHistories.Add(history);
        await _context.SaveChangesAsync();
        return history;
    }

    public async Task Update(ManufacturingOrderHistoryModel history)
    {
        _context.Entry(history).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    
    public async Task Delete(int plantFloorSectionId, int manufacturingOrderId)
    {
        var history = await GetById(plantFloorSectionId, manufacturingOrderId);
        _context.ManufacturingOrderHistories.Remove(history);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int plantFloorSectionId, int manufacturingOrderId)
    {
        return await _context.ManufacturingOrderHistories.AnyAsync(m => m.PlantFloorSectionId == plantFloorSectionId && m.ManufacturingOrderId == manufacturingOrderId);
    }
}
