using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<PagedResultDTO<ProductDTO>> GetPaged(
        int page, int pageSize, string? search, int? modelId, DateTime? dateFrom, DateTime? dateTo)
    {
        var paged = await _repo.GetPaged(page, pageSize, search, modelId, dateFrom, dateTo);
        return new PagedResultDTO<ProductDTO>
        {
            Data = paged.Data.Select(MapToDTO),
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    public async Task<ProductDTO?> GetById(int id)
    {
        var item = await _repo.GetById(id);
        return item == null ? null : MapToDTO(item);
    }

    public async Task<IEnumerable<ProductDTO>> GetByManufacturingOrder(int orderId)
    {
        var items = await _repo.GetByManufacturingOrder(orderId);
        return items.Select(MapToDTO);
    }

    public async Task<ProductDTO> Create(CreateProductDTO dto)
    {
        var entity = new ProductModel
        {
            ManufacturingOrderId = dto.ManufacturingOrderId,
            ModelId = dto.ModelId,
            SerialNumber = dto.SerialNumber,
            LotNumber = dto.LotNumber,
        };
        var created = await _repo.Create(entity);
        return MapToDTO(created);
    }

    public async Task<bool> Update(int id, UpdateProductDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return false;
        if (dto.ProductionDate != null) entity.ProductionDate = dto.ProductionDate;
        await _repo.Update(entity);
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await _repo.Exists(id)) return false;
        await _repo.Delete(id);
        return true;
    }

    private static ProductDTO MapToDTO(ProductModel p) =>
        new ProductDTO(p.Id, p.ManufacturingOrderId, p.ModelId, p.CarModel?.Name, p.SerialNumber, p.LotNumber, p.ProductionDate);
}