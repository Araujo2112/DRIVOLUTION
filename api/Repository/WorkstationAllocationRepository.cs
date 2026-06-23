using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace Drivolution.Repository;
public class WorkstationAllocationRepository : IWorkstationAllocationRepository
{
    private readonly ApplicationDbContext _context;
    public WorkstationAllocationRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<WorkstationAllocationModel>> GetAll() =>
        await _context.WorkstationAllocations.Include(wa => wa.Resource).Include(wa => wa.Workstation).ToListAsync();
    public async Task<IEnumerable<WorkstationAllocationModel>> GetByWorkstation(int workstationId) =>
        await _context.WorkstationAllocations.Where(wa => wa.WorkstationId == workstationId).Include(wa => wa.Resource).ToListAsync();
    public async Task<WorkstationAllocationModel?> GetById(int id) => await _context.WorkstationAllocations.FindAsync(id);
    public async Task<WorkstationAllocationModel> Create(WorkstationAllocationModel entity)
    {
        _context.WorkstationAllocations.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(WorkstationAllocationModel entity)
    {
        _context.WorkstationAllocations.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.WorkstationAllocations.FindAsync(id);
        if (entity != null) { _context.WorkstationAllocations.Remove(entity); await _context.SaveChangesAsync(); }
    }
}
