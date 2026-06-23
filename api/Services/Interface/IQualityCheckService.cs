using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IQualityCheckService
{
    Task<IEnumerable<QualityCheckDTO>> GetByProduct(int productId);
    Task<QualityCheckDTO?> GetById(int id);
    Task<QualityCheckDTO> Create(CreateQualityCheckDTO dto);
}