using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.ProductLot;

public interface IProductLotService
{
    Task<IEnumerable<ProductLotDTO>> GetAllProductLots();
    Task<ProductLotDTO> GetProductLotById(int id);
    Task<ProductLotDTO> CreateProductLot(CreateProductLotDTO productLotDto);
    Task<ProductLotDTO> UpdateProductLot(int id, UpdateProductLotWithIdDTO productLotDto);
    Task DeleteProductLot(int id);
}