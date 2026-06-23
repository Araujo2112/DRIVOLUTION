using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IQualityCheckRepository
{
    Task<IEnumerable<QualityCheckModel>> GetByProduct(int productId);
    Task<QualityCheckModel?> GetById(int id);
    Task<QualityCheckModel> Create(QualityCheckModel entity);
    Task Update(QualityCheckModel entity);
}
