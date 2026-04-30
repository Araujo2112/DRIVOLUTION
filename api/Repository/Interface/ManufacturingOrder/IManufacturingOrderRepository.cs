using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.ManufacturingOrder;

public interface IManufacturingOrderRepository
{
    Task<IEnumerable<ManufacturingOrderModel>> GetAll();
    Task<ManufacturingOrderModel> GetById(int id);
    Task<ManufacturingOrderModel> Create(ManufacturingOrderModel manufacturingOrder);
    Task Update(ManufacturingOrderModel manufacturingOrder);
    Task Delete(int id);
    Task<bool> Exists(int id);

    Task<ManufacturingOrderModel?> GetByIdWithDetailsForGraphAsync(int manufacturingOrderId);
}