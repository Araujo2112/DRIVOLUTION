using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.ManufacturingOrder;

public interface IManufacturingOrderService
{
    Task<IEnumerable<ManufacturingOrderDTO>> GetAllOrders();
    Task<ManufacturingOrderDTO> GetOrderById(int id);
    Task<ManufacturingOrderDTO> CreateOrder(CreateManufacturingOrderDTO orderDto);
    Task<ManufacturingOrderDTO> UpdateOrder(int id, UpdateManufacturingOrderDTO orderDto);
    Task DeleteOrder(int id);

    Task<GraphDto> BuildGraphAsync(int manufacturingOrderId);
}