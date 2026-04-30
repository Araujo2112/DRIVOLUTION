using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.ManufacturingPhase;

public interface IManufacturingPhaseRepository
{
    Task<IEnumerable<ManufacturingPhaseModel>> GetAll();
    Task<ManufacturingPhaseModel> GetById(int id);
    Task<ManufacturingPhaseModel> Create(ManufacturingPhaseModel manufacturingPhase);
    Task Update(ManufacturingPhaseModel manufacturingPhase);
    Task Delete(int id);
    Task<bool> Exists(int id);
}