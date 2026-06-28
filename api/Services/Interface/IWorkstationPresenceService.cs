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
}