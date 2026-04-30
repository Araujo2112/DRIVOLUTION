using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.ManufacturingOrderHistory;

public interface IManufacturingOrderHistoryService
{
    Task<IEnumerable<ManufacturingOrderHistoryDTO>> GetAllHistory();
    Task<ManufacturingOrderHistoryDTO> GetHistoryById(int manufacturingOrderId, int PlantFloorSectionId);
    Task<ManufacturingOrderHistoryDTO> CreateHistory(CreateManufacturingOrderHistoryDTO historyDto);

    Task<ManufacturingOrderHistoryDTO> UpdateHistory(int manufacturingOrderId, int PlantFloorSectionId,
        UpdateManufacturingOrderHistoryDTO historyDto);

    Task DeleteHistory(int manufacturingOrderId, int PlantFloorSectionId);
}