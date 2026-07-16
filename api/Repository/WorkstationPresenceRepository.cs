using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir os registos de presença dos operadores
// nas diferentes workstations da linha de produção
public class WorkstationPresenceRepository : IWorkstationPresenceRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _db;

    // O ASP.NET injeta automaticamente o DbContext
    public WorkstationPresenceRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    // Procura um registo de presença pelo seu ID,
    // incluindo o utilizador, a workstation e a fase associada
    public async Task<WorkstationPresenceModel?> GetById(int id)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(p => p.Id == id);

    // Devolve o histórico de presenças de uma workstation,
    // ordenado da mais recente para a mais antiga
    public async Task<IEnumerable<WorkstationPresenceModel>> GetByWorkstation(int workstationId)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .Where(p => p.WorkstationId == workstationId)
            .OrderByDescending(p => p.CheckedInAt)
            .ToListAsync();

    // Devolve o histórico de presenças de um utilizador,
    // ordenado da mais recente para a mais antiga
    public async Task<IEnumerable<WorkstationPresenceModel>> GetByUser(int appUserId)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .Where(p => p.AppUserId == appUserId)
            .OrderByDescending(p => p.CheckedInAt)
            .ToListAsync();

    // Procura a presença ativa de um utilizador numa workstation específica
    public async Task<WorkstationPresenceModel?> GetActiveByUserAndWorkstation(int appUserId, int workstationId)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(p =>
                p.AppUserId == appUserId &&
                p.WorkstationId == workstationId &&
                p.CheckedOutAt == null);

    // Presença ativa do utilizador, independentemente da workstation.
    // Usado pelo fluxo FIWARE (badge) para saber se o operador tem de fazer
    // check-out automático de um posto anterior antes de entrar noutro.
    public async Task<WorkstationPresenceModel?> GetActiveByUser(int appUserId)
        => await _db.WorkstationPresences
            .Include(p => p.AppUser)
            .Include(p => p.Workstation)
                .ThenInclude(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(p => p.AppUserId == appUserId && p.CheckedOutAt == null);

    // Cria um novo registo de presença
    public async Task<WorkstationPresenceModel> Create(WorkstationPresenceModel entity)
    {
        // Adiciona o registo à base de dados
        _db.WorkstationPresences.Add(entity);

        // Guarda as alterações
        await _db.SaveChangesAsync();

        // Recarrega a entidade com todas as relações incluídas
        return (await GetById(entity.Id))!;
    }

    // Atualiza um registo de presença existente
    public async Task Update(WorkstationPresenceModel entity)
    {
        // Marca a entidade como alterada
        _db.WorkstationPresences.Update(entity);

        // Guarda as alterações
        await _db.SaveChangesAsync();
    }
}