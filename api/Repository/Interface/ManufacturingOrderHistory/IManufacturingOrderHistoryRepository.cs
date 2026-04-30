using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.ManufacturingOrderHistory;

public interface IManufacturingOrderHistoryRepository
{
    Task<IEnumerable<ManufacturingOrderHistoryModel>> GetAll();
    Task<ManufacturingOrderHistoryModel> GetById(int plantFloorSectionId, int manufacturingOrderId);
    Task<ManufacturingOrderHistoryModel> Create(ManufacturingOrderHistoryModel history);
    Task Update(ManufacturingOrderHistoryModel history);
    Task Delete(int plantFloorSectionId, int manufacturingOrderId);
    Task<bool> Exists(int plantFloorSectionId, int manufacturingOrderId);
}