using Drivolution.DTO;
using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IManufacturingOrderRepository
{
    Task<PagedResultDTO<ManufacturingOrderModel>> GetPaged(int page, int pageSize, string? search, string? status, DateTime? dateFrom, DateTime? dateTo);
    Task<ManufacturingOrderModel?> GetById(int id);
    Task<ManufacturingOrderModel?> GetByIdWithDetails(int id);
    Task<ManufacturingOrderModel> Create(ManufacturingOrderModel entity);
    Task Update(ManufacturingOrderModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}