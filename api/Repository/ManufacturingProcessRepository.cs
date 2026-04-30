using ApiTexPact.Data;
using ApiTexPact.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public interface IManufacturingProcessRepository
{
    Task<IEnumerable<ManufacturingProcessModel>> GetAll();
    Task<ManufacturingProcessModel> GetById(int id);
    Task<ManufacturingProcessModel> Create(ManufacturingProcessModel manufacturingProcess);
    Task Update(ManufacturingProcessModel manufacturingProcess);
    Task Delete(int id);
    Task<bool> Exists(int id);
}

public class ManufacturingProcessRepository : IManufacturingProcessRepository
{
    private readonly ApplicationDbContext _context;

    public ManufacturingProcessRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ManufacturingProcessModel>> GetAll()
    {
        return await _context.ManufacturingProcesses
            .Include(mp => mp.Product)
            .ToListAsync();
    }

    public async Task<ManufacturingProcessModel> GetById(int id)
    {
        var manufacturingProcess = await _context.ManufacturingProcesses
            .Include(mp => mp.Product)
            .FirstOrDefaultAsync(mp => mp.Id == id);

        if (manufacturingProcess == null)
            throw new KeyNotFoundException($"Manufacturing Process with ID {id} not found");

        return manufacturingProcess;
    }

    public async Task<ManufacturingProcessModel> Create(ManufacturingProcessModel manufacturingProcess)
    {
        _context.ManufacturingProcesses.Add(manufacturingProcess);
        await _context.SaveChangesAsync();
        return manufacturingProcess;
    }

    public async Task Update(ManufacturingProcessModel manufacturingProcess)
    {
        _context.Entry(manufacturingProcess).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var manufacturingProcess = await GetById(id);
        _context.ManufacturingProcesses.Remove(manufacturingProcess);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.ManufacturingProcesses.AnyAsync(mp => mp.Id == id);
    }
}