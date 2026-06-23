using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IWorkstationStatusRepository
{
    Task<IEnumerable<WorkstationStatusModel>> GetByWorkstation(int workstationId);
    Task<WorkstationStatusModel?> GetLatestByWorkstation(int workstationId);
    Task<WorkstationStatusModel> Create(WorkstationStatusModel entity);
}
