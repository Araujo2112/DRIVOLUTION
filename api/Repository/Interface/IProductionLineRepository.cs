using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IProductionLineRepository
{
    Task<IEnumerable<ProductionLineModel>> GetAll();
    Task<ProductionLineModel?> GetById(int id);
    Task<ProductionLineModel> Create(ProductionLineModel entity);
    Task Update(ProductionLineModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
