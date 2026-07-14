using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IWorkstationPresenceService
{
    /// <summary>Regista check-in do utilizador autenticado na workstation.</summary>
    Task<(bool Success, string? Error, WorkstationPresenceDTO? Result)> CheckIn(int appUserId, int workstationId);

    /// <summary>Regista check-out do utilizador autenticado na workstation.</summary>
    Task<(bool Success, string? Error)> CheckOut(int appUserId, int workstationId);

    /// <summary>Histórico de presenças de uma workstation, com produtos cruzados.</summary>
    Task<IEnumerable<WorkstationPresenceDetailDTO>> GetByWorkstation(int workstationId);

    /// <summary>Histórico de presenças de um utilizador.</summary>
    Task<IEnumerable<WorkstationPresenceDTO>> GetByUser(int appUserId);

    /// <summary>Presença ativa do utilizador na workstation (null se não estiver).</summary>
    Task<WorkstationPresenceDTO?> GetActive(int appUserId, int workstationId);

    /// <summary>
    /// Processa um evento de leitura de crachá (FIWARE/Badge) para um utilizador numa workstation:
    /// - Se já tem check-in ativo NESSA workstation → check-out (tap de saída no mesmo posto).
    /// - Se tem check-in ativo NOUTRA workstation → check-out automático aí e check-in na nova.
    /// - Caso contrário → check-in simples.
    /// Não depende de sessão/JWT — o appUserId vem resolvido do device FIWARE.
    /// </summary>
    Task<(bool Success, string? Error, string Action)> ProcessBadgeScan(int appUserId, int workstationId);
}