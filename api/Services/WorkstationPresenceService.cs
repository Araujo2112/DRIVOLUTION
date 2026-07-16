using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Services;

// Service responsável pela lógica de presença dos operadores nas workstations
public class WorkstationPresenceService : IWorkstationPresenceService
{
    // Repository responsável pelos registos de presença
    private readonly IWorkstationPresenceRepository _presenceRepo;

    // Contexto da base de dados, usado para consultas adicionais
    private readonly ApplicationDbContext _db;

    // O ASP.NET injeta automaticamente o repository e o DbContext
    public WorkstationPresenceService(
        IWorkstationPresenceRepository presenceRepo,
        ApplicationDbContext db)
    {
        _presenceRepo = presenceRepo;
        _db = db;
    }

    // Regista a entrada de um utilizador numa workstation
    public async Task<(bool Success, string? Error, WorkstationPresenceDTO? Result)> CheckIn(
        int appUserId,
        int workstationId)
    {
        // 1. Verificar se a workstation existe e é elegível (human ou hybrid)
        var workstation = await _db.Workstations
            // Inclui a fase de fabrico associada à workstation
            .Include(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(w => w.Id == workstationId);

        // Se a workstation não existir, termina com erro
        if (workstation == null)
            return (false, "Workstation não encontrada.", null);

        // Apenas workstations human ou hybrid permitem presença de operadores
        if (!WorkstationKind.HumanEligible.Contains(workstation.Kind))
            return (
                false,
                $"Esta workstation é do tipo '{workstation.Kind ?? "não definido"}'. Apenas workstations 'human' ou 'hybrid' suportam presença de operadores.",
                null
            );

        // 2. Verificar se o utilizador já tem check-in ativo nesta workstation
        var existing = await _presenceRepo.GetActiveByUserAndWorkstation(
            appUserId,
            workstationId
        );

        // Evita dois check-ins ativos do mesmo utilizador na mesma workstation
        if (existing != null)
            return (
                false,
                "Já existe um check-in ativo nesta workstation para este utilizador. Faz check-out primeiro.",
                null
            );

        // 3. Verificar se o utilizador já tem check-in ativo noutras workstations
        var activeElsewhere = await _db.WorkstationPresences
            .Include(p => p.Workstation)
            .FirstOrDefaultAsync(p =>
                p.AppUserId == appUserId &&
                p.CheckedOutAt == null &&
                p.WorkstationId != workstationId
            );

        // Um operador só pode estar presente numa workstation de cada vez
        if (activeElsewhere != null)
            return (
                false,
                $"Já tens check-in ativo na workstation '{activeElsewhere.Workstation?.Type ?? activeElsewhere.WorkstationId.ToString()}'. Um operador só pode estar presente numa workstation de cada vez.",
                null
            );

        // 4. Criar presença

        // Confirma que o utilizador existe
        var user = await _db.AppUsers.FindAsync(appUserId);

        if (user == null)
            return (false, "Utilizador não encontrado.", null);

        // Cria o novo registo de presença
        var presence = new WorkstationPresenceModel
        {
            AppUserId = appUserId,
            WorkstationId = workstationId,
            CheckedInAt = DateTime.UtcNow
        };

        // Guarda a presença
        var created = await _presenceRepo.Create(presence);

        // Devolve sucesso e o registo criado convertido para DTO
        return (true, null, ToDTO(created));
    }

    // Regista a saída de um utilizador de uma workstation
    public async Task<(bool Success, string? Error)> CheckOut(
        int appUserId,
        int workstationId)
    {
        // Procura uma presença ativa do utilizador nesta workstation
        var active = await _presenceRepo.GetActiveByUserAndWorkstation(
            appUserId,
            workstationId
        );

        // Se não existir presença ativa, não é possível fazer check-out
        if (active == null)
            return (
                false,
                "Não existe check-in ativo nesta workstation para este utilizador."
            );

        // Regista a data e hora de saída
        active.CheckedOutAt = DateTime.UtcNow;

        // Guarda a alteração
        await _presenceRepo.Update(active);

        return (true, null);
    }

    // Devolve o histórico de presenças de uma workstation,
    // incluindo os produtos que passaram durante cada presença
    public async Task<IEnumerable<WorkstationPresenceDetailDTO>> GetByWorkstation(
        int workstationId)
    {
        // Obtém as presenças da workstation
        var presences = await _presenceRepo.GetByWorkstation(workstationId);

        var result = new List<WorkstationPresenceDetailDTO>();

        foreach (var p in presences)
        {
            // Descobre que produtos passaram pela workstation
            // durante o intervalo de presença do operador
            var products = await GetCrossedProducts(p);

            // Cria o DTO detalhado
            result.Add(ToDetailDTO(p, products));
        }

        return result;
    }

    // Devolve o histórico de presenças de um utilizador
    public async Task<IEnumerable<WorkstationPresenceDTO>> GetByUser(int appUserId)
    {
        var presences = await _presenceRepo.GetByUser(appUserId);

        // Converte todos os registos para DTO
        return presences.Select(ToDTO);
    }

    // Devolve a presença ativa de um utilizador numa workstation
    public async Task<WorkstationPresenceDTO?> GetActive(
        int appUserId,
        int workstationId)
    {
        var active = await _presenceRepo.GetActiveByUserAndWorkstation(
            appUserId,
            workstationId
        );

        return active == null ? null : ToDTO(active);
    }

    // ─── Evento FIWARE (Badge) ────────────────────────────────────────────────

    // Processa uma leitura de crachá recebida através do FIWARE
    public async Task<(bool Success, string? Error, string Action)> ProcessBadgeScan(
        int appUserId,
        int workstationId)
    {
        // 1. Tap no mesmo posto onde já está presente → saída
        var activeHere = await _presenceRepo.GetActiveByUserAndWorkstation(
            appUserId,
            workstationId
        );

        if (activeHere != null)
        {
            // Se já está nesta workstation, a leitura funciona como check-out
            var (ok, err) = await CheckOut(appUserId, workstationId);

            return (ok, err, "checkout");
        }

        // 2. Presença ativa noutra workstation → sai automaticamente de lá primeiro
        //    (o operador "levou o crachá" para o novo posto sem passar antes na saída)
        var activeElsewhere = await _presenceRepo.GetActiveByUser(appUserId);

        if (activeElsewhere != null)
        {
            // Fecha automaticamente a presença anterior
            await CheckOut(
                appUserId,
                activeElsewhere.WorkstationId
            );
        }

        // 3. Entrada no novo posto
        var (success, error, _) = await CheckIn(
            appUserId,
            workstationId
        );

        return (success, error, "checkin");
    }

    // ─── Cruzamento presença ↔ produtos ──────────────────────────────────────

    // Obtém os produtos que passaram pela workstation
    // durante o período em que o operador esteve presente
    private async Task<IEnumerable<PresenceProductCrossDTO>> GetCrossedProducts(
        WorkstationPresenceModel presence)
    {
        // Início da presença
        var checkIn = presence.CheckedInAt;

        // Se ainda não houve check-out, utiliza o momento atual
        var checkOut = presence.CheckedOutAt ?? DateTime.UtcNow;

        // Produto passou pela mesma workstation E o intervalo de fase sobrepõe a presença
        var phases = await _db.ProductPhases
            .Include(pp => pp.Product)
            .Where(pp =>
                pp.WorkstationId == presence.WorkstationId &&

                // A fase começou antes do operador sair
                pp.DatetimeIni < checkOut &&

                // A fase ainda está aberta ou terminou depois do operador entrar
                (pp.DatetimeEnd == null || pp.DatetimeEnd > checkIn)
            )
            .ToListAsync();

        // Converte cada fase encontrada num DTO com informação do produto
        return phases.Select(pp => new PresenceProductCrossDTO(
            pp.ProductId,
            pp.Product?.SerialNumber ?? pp.ProductId.ToString(),
            pp.DatetimeIni,
            pp.DatetimeEnd
        ));
    }

    // ─── Mappers ─────────────────────────────────────────────────────────────

    // Converte uma presença para o DTO normal
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

    // Converte uma presença para o DTO detalhado,
    // incluindo os produtos que passaram durante essa presença
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