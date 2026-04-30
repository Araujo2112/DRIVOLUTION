using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.ManufacturingProcessPhase;

public interface IManufacturingProcessPhaseRepository
{
    Task<IEnumerable<ManufacturingProcessPhaseModel>> GetAll();
    Task<ManufacturingProcessPhaseModel> GetById(int manufacturingProcessId, int manufacturingPhaseId);
    Task<ManufacturingProcessPhaseModel> Create(ManufacturingProcessPhaseModel manufacturingProcessPhase);
    Task Update(ManufacturingProcessPhaseModel manufacturingProcessPhase);
    Task Delete(int manufacturingProcessId, int manufacturingPhaseId);
    Task<bool> Exists(int manufacturingProcessId, int manufacturingPhaseId);
}