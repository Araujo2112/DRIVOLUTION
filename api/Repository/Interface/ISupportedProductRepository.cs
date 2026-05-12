using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface;
public interface ISupportedProductRepository
{
    Task<IEnumerable<SupportedProductModel>> GetBySupport(int supportId);
    Task<SupportedProductModel?> GetCurrentBySupport(int supportId);
    Task<SupportedProductModel> Create(SupportedProductModel entity);
    Task Update(SupportedProductModel entity);
}
