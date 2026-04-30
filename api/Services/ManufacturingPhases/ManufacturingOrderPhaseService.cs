using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingOrderPhase;
using ApiTexPact.Services.Interface.ManufacturingOrderPhase;

namespace ApiTexPact.Services;

public class ManufacturingOrderPhaseService : IManufacturingOrderPhaseService
{
    private readonly IManufacturingOrderPhaseRepository _repository;

    public ManufacturingOrderPhaseService(IManufacturingOrderPhaseRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ManufacturingOrderPhaseDTO>> GetAllPhases()
    {
        var phases = await _repository.GetAll();
        return phases.Select(ToDTO);
    }

    public async Task<ManufacturingOrderPhaseDTO> GetPhaseById(int id)
    {
        var phase = await _repository.GetById(id);
        return ToDTO(phase);
    }

    public async Task<ManufacturingOrderPhaseDTO> CreatePhase(CreateManufacturingOrderPhaseDTO phaseDto)
    {
        var phase = new ManufacturingOrderPhaseModel
        {
            CustomizationParams = phaseDto.CustomizationParams,
            Quantity = phaseDto.Quantity,
            SheduleInit = phaseDto.SheduleInit,
            DateTimeInit = phaseDto.DateTimeInit,
            DateTimeEnd = phaseDto.DateTimeEnd,
            ManufacturingOrderId = phaseDto.ManufacturingOrderId,
            ManufacturingPhaseId = phaseDto.ManufacturingPhaseId
        };

        var created = await _repository.Create(phase);
        return ToDTO(created);
    }

    public async Task<ManufacturingOrderPhaseDTO> UpdatePhase(int id, UpdateManufacturingOrderPhaseDTO phaseDto)
    {
        var existingPhase = await _repository.GetById(id);

        existingPhase.CustomizationParams = phaseDto.CustomizationParams;
        existingPhase.Quantity = phaseDto.Quantity;
        existingPhase.SheduleInit = phaseDto.SheduleInit;
        existingPhase.DateTimeInit = phaseDto.DateTimeInit;
        existingPhase.DateTimeEnd = phaseDto.DateTimeEnd;
        existingPhase.ManufacturingOrderId = phaseDto.ManufacturingOrderId;
        existingPhase.ManufacturingPhaseId = phaseDto.ManufacturingPhaseId;

        await _repository.Update(existingPhase);
        return ToDTO(existingPhase);
    }

    public async Task DeletePhase(int id)
    {
        await _repository.Delete(id);
    }

    private static ManufacturingOrderPhaseDTO ToDTO(ManufacturingOrderPhaseModel phase)
    {
        return new ManufacturingOrderPhaseDTO
        {
            Id = phase.Id,
            CustomizationParams = phase.CustomizationParams,
            Quantity = phase.Quantity,
            SheduleInit = phase.SheduleInit,
            DateTimeInit = phase.DateTimeInit,
            DateTimeEnd = phase.DateTimeEnd,
            ManufacturingOrderId = phase.ManufacturingOrderId,
            ManufacturingPhaseId = phase.ManufacturingPhaseId
        };
    }
}