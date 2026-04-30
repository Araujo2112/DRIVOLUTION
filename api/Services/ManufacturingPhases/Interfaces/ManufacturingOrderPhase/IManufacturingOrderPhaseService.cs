using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.ManufacturingOrderPhase;

public interface IManufacturingOrderPhaseService
{
    Task<IEnumerable<ManufacturingOrderPhaseDTO>> GetAllPhases();
    Task<ManufacturingOrderPhaseDTO> GetPhaseById(int id);
    Task<ManufacturingOrderPhaseDTO> CreatePhase(CreateManufacturingOrderPhaseDTO phaseDto);
    Task<ManufacturingOrderPhaseDTO> UpdatePhase(int id, UpdateManufacturingOrderPhaseDTO phaseDto);
    Task DeletePhase(int id);
}