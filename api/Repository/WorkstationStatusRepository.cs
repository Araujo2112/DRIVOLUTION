using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir o histórico de estados das workstations
public class WorkstationStatusRepository : IWorkstationStatusRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public WorkstationStatusRepository(ApplicationDbContext context) => _context = context;

    // Devolve o histórico de estados de uma workstation,
    // ordenado do mais recente para o mais antigo
    public async Task<IEnumerable<WorkstationStatusModel>> GetByWorkstation(int workstationId) =>
        await _context.WorkstationStatuses
            .Where(ws => ws.WorkstationId == workstationId)
            .OrderByDescending(ws => ws.Timestamp)
            .ToListAsync();

    // Devolve o estado mais recente de uma workstation
    public async Task<WorkstationStatusModel?> GetLatestByWorkstation(int workstationId) =>
        await _context.WorkstationStatuses
            .Where(ws => ws.WorkstationId == workstationId)
            .OrderByDescending(ws => ws.Timestamp)
            .FirstOrDefaultAsync();

    // Cria um novo registo de estado para uma workstation
    public async Task<WorkstationStatusModel> Create(WorkstationStatusModel entity)
    {
        // Adiciona o novo estado à base de dados
        _context.WorkstationStatuses.Add(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();

        return entity;
    }
}