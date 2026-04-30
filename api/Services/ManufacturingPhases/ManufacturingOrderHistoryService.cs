using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository;
using ApiTexPact.Repository.Interface.ManufacturingOrderHistory;
using ApiTexPact.Services.Interface.ManufacturingOrderHistory;

namespace ApiTexPact.Services;



public class ManufacturingOrderHistoryService : IManufacturingOrderHistoryService
{
    private readonly IManufacturingOrderHistoryRepository _repository;

    public ManufacturingOrderHistoryService(IManufacturingOrderHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ManufacturingOrderHistoryDTO>> GetAllHistory()
    {
        var history = await _repository.GetAll();
        return history.Select(ToDTO);
    }

    public async Task<ManufacturingOrderHistoryDTO?> GetHistoryById(int manufacturingOrderId, int plantFloorSectionId)
    {
        var history = await _repository.GetById(manufacturingOrderId, plantFloorSectionId);
    
        if (history == null)
            return null;

        return ToDTO(history);
    }


    public async Task<ManufacturingOrderHistoryDTO> CreateHistory(CreateManufacturingOrderHistoryDTO historyDto)
    {
        var history = new ManufacturingOrderHistoryModel
        {
            ManufacturingOrderId = historyDto.ManufacturingOrderId,
            PlantFloorSectionId = historyDto.PlantFloorSectionId,
            DateTime = historyDto.DateTime,
            StatusName = historyDto.StatusName
        };

        var created = await _repository.Create(history);
        return ToDTO(created);
    }

    public async Task<ManufacturingOrderHistoryDTO> UpdateHistory(int manufacturingOrderId, int PlantFloorSectionId,
        UpdateManufacturingOrderHistoryDTO historyDto)
    {
        var existingHistory = await _repository.GetById(manufacturingOrderId, PlantFloorSectionId);

        existingHistory.DateTime = historyDto.DateTime;
        existingHistory.StatusName = historyDto.StatusName;

        await _repository.Update(existingHistory);
        return ToDTO(existingHistory);
    }

    public async Task DeleteHistory(int manufacturingOrderId, int PlantFloorSectionId)
    {
        await _repository.Delete(manufacturingOrderId, PlantFloorSectionId);
    }

    private static ManufacturingOrderHistoryDTO ToDTO(ManufacturingOrderHistoryModel history)
    {
        return new ManufacturingOrderHistoryDTO
        {
            ManufacturingOrderId = history.ManufacturingOrderId,
            PlantFloorSectionId = history.PlantFloorSectionId,
            DateTime = history.DateTime,
            StatusName = history.StatusName
        };
    }
}