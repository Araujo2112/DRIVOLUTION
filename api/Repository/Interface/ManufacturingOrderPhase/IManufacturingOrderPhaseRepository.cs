using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.ManufacturingOrderPhase;

public interface IManufacturingOrderPhaseRepository
{
    Task<IEnumerable<ManufacturingOrderPhaseModel>> GetAll();
    Task<ManufacturingOrderPhaseModel> GetById(int id);
    Task<ManufacturingOrderPhaseModel> Create(ManufacturingOrderPhaseModel manufacturingOrderPhase);
    Task Update(ManufacturingOrderPhaseModel manufacturingOrderPhase);
    Task Delete(int id);
    Task<bool> Exists(int id);
}