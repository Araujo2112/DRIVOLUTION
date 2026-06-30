using Drivolution.DTO;
using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IProductRepository
{
    Task<PagedResultDTO<ProductModel>> GetPaged(int page, int pageSize, string? search, int? modelId, DateTime? dateFrom, DateTime? dateTo);
    Task<ProductModel?> GetById(int id);
    Task<IEnumerable<ProductModel>> GetByManufacturingOrder(int orderId);
    Task<ProductModel> Create(ProductModel entity);
    Task Update(ProductModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}