using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IProductPhaseRepository
{
    Task<IEnumerable<ProductPhaseModel>> GetByProduct(int productId);
    Task<ProductPhaseModel?> GetCurrentByProduct(int productId);
    Task<IEnumerable<ProductPhaseModel>> GetAllOpenByProduct(int productId);
    Task<ProductPhaseModel?> GetById(int id);
    Task<ProductPhaseModel> Create(ProductPhaseModel entity);
    Task Update(ProductPhaseModel entity);
    Task<IEnumerable<ProductPhaseModel>> GetOpenPhasesWithPhaseInfoAsync();
    Task<ProductPhaseModel?> GetByIdAsync(int id);
}