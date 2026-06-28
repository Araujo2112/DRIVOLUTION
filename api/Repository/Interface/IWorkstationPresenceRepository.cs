using Drivolution.Models;

namespace Drivolution.Repository.Interface;

public interface IWorkstationPresenceRepository
{
    Task<WorkstationPresenceModel?> GetById(int id);
    Task<IEnumerable<WorkstationPresenceModel>> GetByWorkstation(int workstationId);
    Task<IEnumerable<WorkstationPresenceModel>> GetByUser(int appUserId);
    Task<WorkstationPresenceModel?> GetActiveByUserAndWorkstation(int appUserId, int workstationId);
    Task<WorkstationPresenceModel> Create(WorkstationPresenceModel entity);
    Task Update(WorkstationPresenceModel entity);
}