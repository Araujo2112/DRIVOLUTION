using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.ManufacturingPhase;

public interface IManufacturingPhaseService
{
    Task<IEnumerable<ManufacturingPhaseDTO>> GetAllManufacturingPhases();
    Task<ManufacturingPhaseDTO> GetManufacturingPhaseById(int id);
    Task<ManufacturingPhaseDTO> CreateManufacturingPhase(CreateManufacturingPhaseDTO manufacturingPhaseDto);
    Task<ManufacturingPhaseDTO> UpdateManufacturingPhase(int id, UpdateManufacturingPhaseDTO manufacturingPhaseDto);
    Task DeleteManufacturingPhase(int id);
}