using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IProductPhaseService
{
    Task<IEnumerable<ProductPhaseDTO>> GetByProduct(int productId);
    Task<ProductPhaseDTO?> GetCurrent(int productId);
    Task<ProductPhaseDTO> Create(CreateProductPhaseDTO dto);
    Task Close(int id, CloseProductPhaseDTO dto);
}
