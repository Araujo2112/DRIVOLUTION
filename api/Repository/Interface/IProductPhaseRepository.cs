using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface;
public interface IProductPhaseRepository
{
    Task<IEnumerable<ProductPhaseModel>> GetByProduct(int productId);
    Task<ProductPhaseModel?> GetCurrentByProduct(int productId);
    Task<ProductPhaseModel?> GetById(int id);
    Task<ProductPhaseModel> Create(ProductPhaseModel entity);
    Task Update(ProductPhaseModel entity);
}
