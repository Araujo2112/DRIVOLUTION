using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.WorkstationStatus;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
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
