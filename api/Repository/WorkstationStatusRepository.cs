using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace Drivolution.Repository;
public class WorkstationStatusRepository : IWorkstationStatusRepository
{
    private readonly ApplicationDbContext _context;
    public WorkstationStatusRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<WorkstationStatusModel>> GetByWorkstation(int workstationId) =>
        await _context.WorkstationStatuses.Where(ws => ws.WorkstationId == workstationId).OrderByDescending(ws => ws.Timestamp).ToListAsync();
    public async Task<WorkstationStatusModel?> GetLatestByWorkstation(int workstationId) =>
        await _context.WorkstationStatuses.Where(ws => ws.WorkstationId == workstationId).OrderByDescending(ws => ws.Timestamp).FirstOrDefaultAsync();
    public async Task<WorkstationStatusModel> Create(WorkstationStatusModel entity)
    {
        _context.WorkstationStatuses.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
}
