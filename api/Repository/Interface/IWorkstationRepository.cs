using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface;
public interface IWorkstationRepository
{
    Task<IEnumerable<WorkstationModel>> GetAll();
    Task<IEnumerable<WorkstationModel>> GetByProductionLine(int productionLineId);
    Task<WorkstationModel?> GetById(int id);
    Task<WorkstationModel> Create(WorkstationModel entity);
    Task Update(WorkstationModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
