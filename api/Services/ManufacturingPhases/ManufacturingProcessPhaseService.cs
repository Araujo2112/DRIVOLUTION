using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingProcessPhase;
using ApiTexPact.Services.Interface.ManufacturingProcessPhase;

namespace ApiTexPact.Services;

public class ManufacturingProcessPhaseService : IManufacturingProcessPhaseService
{
    private readonly IManufacturingProcessPhaseRepository _repository;

    public ManufacturingProcessPhaseService(IManufacturingProcessPhaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ManufacturingProcessPhaseDTO>> GetAllManufacturingProcessPhases()
    {
        var manufacturingProcessPhases = await _repository.GetAll();
        return manufacturingProcessPhases.Select(ToDTO);
    }

    public async Task<ManufacturingProcessPhaseDTO> GetManufacturingProcessPhaseById(int manufacturingProcessId,
        int manufacturingPhaseId)
    {
        var manufacturingProcessPhase = await _repository.GetById(manufacturingProcessId, manufacturingPhaseId);
        return ToDTO(manufacturingProcessPhase);
    }

    public async Task<ManufacturingProcessPhaseDTO> CreateManufacturingProcessPhase(
        CreateManufacturingProcessPhaseDTO manufacturingProcessPhaseDto)
    {
        var manufacturingProcessPhase = new ManufacturingProcessPhaseModel
        {
            ManufacturingProcessId = manufacturingProcessPhaseDto.ManufacturingProcessId,
            ManufacturingPhaseId = manufacturingProcessPhaseDto.ManufacturingPhaseId,
            NumberStepOrder = manufacturingProcessPhaseDto.NumberStepOrder
        };

        var created = await _repository.Create(manufacturingProcessPhase);
        return ToDTO(created);
    }

    public async Task<ManufacturingProcessPhaseDTO> UpdateManufacturingProcessPhase(int manufacturingProcessId,
        int manufacturingPhaseId, UpdateManufacturingProcessPhaseDTO manufacturingProcessPhaseDto)
    {
        var existing = await _repository.GetById(manufacturingProcessId, manufacturingPhaseId);

        existing.NumberStepOrder = manufacturingProcessPhaseDto.NumberStepOrder;

        await _repository.Update(existing);
        return ToDTO(existing);
    }

    public async Task DeleteManufacturingProcessPhase(int manufacturingProcessId, int manufacturingPhaseId)
    {
        await _repository.Delete(manufacturingProcessId, manufacturingPhaseId);
    }

    private static ManufacturingProcessPhaseDTO ToDTO(ManufacturingProcessPhaseModel manufacturingProcessPhase)
    {
        return new ManufacturingProcessPhaseDTO
        {
            ManufacturingProcessId = manufacturingProcessPhase.ManufacturingProcessId,
            ManufacturingPhaseId = manufacturingProcessPhase.ManufacturingPhaseId,
            NumberStepOrder = manufacturingProcessPhase.NumberStepOrder,
        };
    }
}