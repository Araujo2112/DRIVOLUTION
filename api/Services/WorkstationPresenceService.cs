using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Services;

public class WorkstationPresenceService : IWorkstationPresenceService
{
    private readonly IWorkstationPresenceRepository _presenceRepo;
    private readonly ApplicationDbContext _db;

    public WorkstationPresenceService(
        IWorkstationPresenceRepository presenceRepo,
        ApplicationDbContext db)
    {
        _presenceRepo = presenceRepo;
        _db = db;
    }

    public async Task<(bool Success, string? Error, WorkstationPresenceDTO? Result)> CheckIn(int appUserId, int workstationId)
    {
        // 1. Verificar se a workstation existe e é elegível (human ou hybrid)
        var workstation = await _db.Workstations
            .Include(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(w => w.Id == workstationId);

        if (workstation == null)
            return (false, "Workstation não encontrada.", null);

        if (!WorkstationKind.HumanEligible.Contains(workstation.Kind))
            return (false, $"Esta workstation é do tipo '{workstation.Kind ?? "não definido"}'. Apenas workstations 'human' ou 'hybrid' suportam presença de operadores.", null);

        // 2. Verificar se o utilizador já tem check-in ativo nesta workstation
        var existing = await _presenceRepo.GetActiveByUserAndWorkstation(appUserId, workstationId);
        if (existing != null)
            return (false, "Já existe um check-in ativo nesta workstation para este utilizador. Faz check-out primeiro.", null);

        // 3. Verificar se o utilizador já tem check-in ativo noutras workstations
        var activeElsewhere = await _db.WorkstationPresences
            .Include(p => p.Workstation)
            .FirstOrDefaultAsync(p => p.AppUserId == appUserId && p.CheckedOutAt == null && p.WorkstationId != workstationId);

        if (activeElsewhere != null)
            return (false, $"Já tens check-in ativo na workstation '{activeElsewhere.Workstation?.Type ?? activeElsewhere.WorkstationId.ToString()}'. Um operador só pode estar presente numa workstation de cada vez.", null);

        // 4. Criar presença
        var user = await _db.AppUsers.FindAsync(appUserId);
        if (user == null)
            return (false, "Utilizador não encontrado.", null);

        var presence = new WorkstationPresenceModel
        {
            AppUserId = appUserId,
            WorkstationId = workstationId,
            CheckedInAt = DateTime.UtcNow
        };

        var created = await _presenceRepo.Create(presence);
        return (true, null, ToDTO(created));
    }

    public async Task<(bool Success, string? Error)> CheckOut(int appUserId, int workstationId)
    {
        var active = await _presenceRepo.GetActiveByUserAndWorkstation(appUserId, workstationId);

        if (active == null)
            return (false, "Não existe check-in ativo nesta workstation para este utilizador.");

        active.CheckedOutAt = DateTime.UtcNow;
        await _presenceRepo.Update(active);

        return (true, null);
    }

    public async Task<IEnumerable<WorkstationPresenceDetailDTO>> GetByWorkstation(int workstationId)
    {
        var presences = await _presenceRepo.GetByWorkstation(workstationId);
        var result = new List<WorkstationPresenceDetailDTO>();

        foreach (var p in presences)
        {
            var products = await GetCrossedProducts(p);
            result.Add(ToDetailDTO(p, products));
        }

        return result;
    }

    public async Task<IEnumerable<WorkstationPresenceDTO>> GetByUser(int appUserId)
    {
        var presences = await _presenceRepo.GetByUser(appUserId);
        return presences.Select(ToDTO);
    }

    public async Task<WorkstationPresenceDTO?> GetActive(int appUserId, int workstationId)
    {
        var active = await _presenceRepo.GetActiveByUserAndWorkstation(appUserId, workstationId);
        return active == null ? null : ToDTO(active);
    }

    // ─── Cruzamento presença ↔ produtos ──────────────────────────────────────

    private async Task<IEnumerable<PresenceProductCrossDTO>> GetCrossedProducts(WorkstationPresenceModel presence)
    {
        var checkIn  = presence.CheckedInAt;
        var checkOut = presence.CheckedOutAt ?? DateTime.UtcNow;

        // Produto passou pela mesma workstation E o intervalo de fase sobrepõe a presença
        var phases = await _db.ProductPhases
            .Include(pp => pp.Product)
            .Where(pp =>
                pp.WorkstationId == presence.WorkstationId &&
                pp.DatetimeIni < checkOut &&
                (pp.DatetimeEnd == null || pp.DatetimeEnd > checkIn))
            .ToListAsync();

        return phases.Select(pp => new PresenceProductCrossDTO(
            pp.ProductId,
            pp.Product?.SerialNumber ?? pp.ProductId.ToString(),
            pp.DatetimeIni,
            pp.DatetimeEnd
        ));
    }

    // ─── Mappers ─────────────────────────────────────────────────────────────

    private static WorkstationPresenceDTO ToDTO(WorkstationPresenceModel p) => new(
        p.Id,
        p.AppUserId,
        p.AppUser?.Name ?? string.Empty,
        p.AppUser?.Email ?? string.Empty,
        p.WorkstationId,
        p.Workstation?.Type,
        p.Workstation?.ManufacturingPhase?.Name,
        p.CheckedInAt,
        p.CheckedOutAt
    );

    private static WorkstationPresenceDetailDTO ToDetailDTO(
        WorkstationPresenceModel p,
        IEnumerable<PresenceProductCrossDTO> products) => new(
        p.Id,
        p.AppUserId,
        p.AppUser?.Name ?? string.Empty,
        p.AppUser?.Email ?? string.Empty,
        p.WorkstationId,
        p.Workstation?.Type,
        p.Workstation?.ManufacturingPhase?.Name,
        p.CheckedInAt,
        p.CheckedOutAt,
        products
    );
}