using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IProductRepository
{
    Task<IEnumerable<ProductModel>> GetAll();
    Task<ProductModel?> GetById(int id);
    Task<IEnumerable<ProductModel>> GetByManufacturingOrder(int orderId);
    Task<ProductModel> Create(ProductModel entity);
    Task Update(ProductModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
