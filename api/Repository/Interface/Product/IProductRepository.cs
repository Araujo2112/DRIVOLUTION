using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.Product;

public interface IProductRepository
{
    Task<IEnumerable<ProductModel>> GetAll();
    Task<ProductModel> GetById(int id);
    Task<ProductModel> Create(ProductModel product);
    Task Update(ProductModel product);
    Task Delete(int id);
    Task<bool> Exists(int id);
}