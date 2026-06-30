using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IManufacturingOrderService
{
    Task<PagedResultDTO<ManufacturingOrderDTO>> GetPaged(int page, int pageSize, string? search, string? status, DateTime? dateFrom, DateTime? dateTo);
    Task<ManufacturingOrderDTO?> GetById(int id);
    Task<ManufacturingOrderDetailDTO?> GetByIdWithDetails(int id);
    Task<ManufacturingOrderDTO> Create(CreateManufacturingOrderDTO dto);
    Task<bool> Update(int id, UpdateManufacturingOrderDTO dto);
    Task<bool> Delete(int id);
}