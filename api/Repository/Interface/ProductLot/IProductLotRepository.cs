using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.ProductLot;

public interface IProductLotRepository
{
    Task<IEnumerable<ProductLotModel>> GetAll();
    Task<ProductLotModel> GetById(int id);
    Task<ProductLotModel> Create(ProductLotModel productLot);
    Task Update(ProductLotModel productLot);
    Task Delete(int id);
    Task<bool> Exists(int id);
}