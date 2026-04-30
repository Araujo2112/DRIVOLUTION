using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.ManufacturingProcessPhase;

public interface IManufacturingProcessPhaseService
{
    Task<IEnumerable<ManufacturingProcessPhaseDTO>> GetAllManufacturingProcessPhases();

    Task<ManufacturingProcessPhaseDTO> GetManufacturingProcessPhaseById(int manufacturingProcessId,
        int manufacturingPhaseId);

    Task<ManufacturingProcessPhaseDTO> CreateManufacturingProcessPhase(
        CreateManufacturingProcessPhaseDTO manufacturingProcessPhaseDto);

    Task<ManufacturingProcessPhaseDTO> UpdateManufacturingProcessPhase(int manufacturingProcessId,
        int manufacturingPhaseId, UpdateManufacturingProcessPhaseDTO manufacturingProcessPhaseDto);

    Task DeleteManufacturingProcessPhase(int manufacturingProcessId, int manufacturingPhaseId);
}