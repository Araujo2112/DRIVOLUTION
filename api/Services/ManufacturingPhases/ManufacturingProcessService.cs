using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository;

namespace ApiTexPact.Services;

public interface IManufacturingProcessService
{
    Task<IEnumerable<ManufacturingProcessDTO>> GetAllManufacturingProcesses();
    Task<ManufacturingProcessDTO> GetManufacturingProcessById(int id);
    Task<ManufacturingProcessDTO> CreateManufacturingProcess(CreateManufacturingProcessDTO manufacturingProcessDto);

    Task<ManufacturingProcessDTO> UpdateManufacturingProcess(int id,
        UpdateManufacturingProcessDTO manufacturingProcessDto);

    Task DeleteManufacturingProcess(int id);
}

public class ManufacturingProcessService : IManufacturingProcessService
{
    private readonly IManufacturingProcessRepository _repository;

    public ManufacturingProcessService(IManufacturingProcessRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ManufacturingProcessDTO>> GetAllManufacturingProcesses()
    {
        var manufacturingProcesses = await _repository.GetAll();
        return manufacturingProcesses.Select(ToDTO);
    }

    public async Task<ManufacturingProcessDTO> GetManufacturingProcessById(int id)
    {
        var manufacturingProcess = await _repository.GetById(id);
        return ToDTO(manufacturingProcess);
    }

    public async Task<ManufacturingProcessDTO> CreateManufacturingProcess(
        CreateManufacturingProcessDTO manufacturingProcessDto)
    {
        var manufacturingProcess = new ManufacturingProcessModel
        {
            ProcessName = manufacturingProcessDto.ProcessName,
            Info = manufacturingProcessDto.Info,
            ProductId = manufacturingProcessDto.ProductId
        };

        var created = await _repository.Create(manufacturingProcess);
        return ToDTO(created);
    }

    public async Task<ManufacturingProcessDTO> UpdateManufacturingProcess(int id,
        UpdateManufacturingProcessDTO manufacturingProcessDto)
    {
        var existing = await _repository.GetById(id);

        existing.ProcessName = manufacturingProcessDto.ProcessName;
        existing.Info = manufacturingProcessDto.Info;

        await _repository.Update(existing);
        return ToDTO(existing);
    }

    public async Task DeleteManufacturingProcess(int id)
    {
        await _repository.Delete(id);
    }

    private static ManufacturingProcessDTO ToDTO(ManufacturingProcessModel manufacturingProcess)
    {
        return new ManufacturingProcessDTO
        {
            Id = manufacturingProcess.Id,
            ProcessName = manufacturingProcess.ProcessName,
            Info = manufacturingProcess.Info,
            ProductId = manufacturingProcess.ProductId
        };
    }
}