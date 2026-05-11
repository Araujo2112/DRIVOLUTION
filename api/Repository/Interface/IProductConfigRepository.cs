using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface.ProductConfig;
public interface IProductConfigRepository
{
    Task<IEnumerable<ProductConfigModel>> GetByProduct(int productId);
    Task<ProductConfigModel> Create(ProductConfigModel entity);
    Task Update(ProductConfigModel entity);
    Task Delete(int id);
}
