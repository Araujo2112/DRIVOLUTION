using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface.ManufacturingOrder;
public interface IManufacturingOrderRepository
{
    Task<IEnumerable<ManufacturingOrderModel>> GetAll();
    Task<ManufacturingOrderModel?> GetById(int id);
    Task<ManufacturingOrderModel?> GetByIdWithDetails(int id);
    Task<IEnumerable<ManufacturingOrderModel>> GetByStatus(string status);
    Task<ManufacturingOrderModel> Create(ManufacturingOrderModel entity);
    Task Update(ManufacturingOrderModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
