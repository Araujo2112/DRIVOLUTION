using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IProductService
{
    Task<PagedResultDTO<ProductDTO>> GetPaged(int page, int pageSize, string? search, int? modelId, DateTime? dateFrom, DateTime? dateTo);
    Task<ProductDTO?> GetById(int id);
    Task<IEnumerable<ProductDTO>> GetByManufacturingOrder(int orderId);
    Task<ProductDTO> Create(CreateProductDTO dto);
    Task<bool> Update(int id, UpdateProductDTO dto);
    Task<bool> Delete(int id);
}