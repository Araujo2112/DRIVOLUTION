using Drivolution.Models;

namespace Drivolution.Repository.Interface;

public interface ISupportedProductRepository
{
    Task<IEnumerable<SupportedProductModel>> GetBySupport(int supportId);
    Task<SupportedProductModel?> GetCurrentBySupport(int supportId);
    Task<SupportedProductModel?> GetById(int id);
    Task<SupportedProductModel> Create(SupportedProductModel entity);
    Task Update(SupportedProductModel entity);
}