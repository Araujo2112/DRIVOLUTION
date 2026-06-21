using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface;
public interface IProductConfigRepository
{
    Task<IEnumerable<ProductConfigModel>> GetByProduct(int productId);
    Task<ProductConfigModel> Create(ProductConfigModel entity);
    Task Update(ProductConfigModel entity);
    Task Delete(int id);
    Task<ProductConfigModel?> GetByProductAndOption(int productId, int configOptionId);
    Task<IEnumerable<ProductConfigModel>> GetByProductAndConfig(int productId, int configId);
}
