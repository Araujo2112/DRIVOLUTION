using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface;
public interface IWorkstationAllocationRepository
{
    Task<IEnumerable<WorkstationAllocationModel>> GetAll();
    Task<IEnumerable<WorkstationAllocationModel>> GetByWorkstation(int workstationId);
    Task<WorkstationAllocationModel?> GetById(int id);
    Task<WorkstationAllocationModel> Create(WorkstationAllocationModel entity);
    Task Update(WorkstationAllocationModel entity);
    Task Delete(int id);
}
