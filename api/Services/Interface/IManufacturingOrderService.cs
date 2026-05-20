using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface;

public interface IManufacturingOrderService
{
    Task<IEnumerable<ManufacturingOrderDTO>> GetAll();
    Task<ManufacturingOrderDTO?> GetById(int id);
    Task<ManufacturingOrderDetailDTO?> GetByIdWithDetails(int id);
    Task<IEnumerable<ManufacturingOrderDTO>> GetByStatus(string status);
    Task<ManufacturingOrderDTO> Create(CreateManufacturingOrderDTO dto);
    Task<bool> Update(int id, UpdateManufacturingOrderDTO dto);
    Task<bool> Delete(int id);
}



