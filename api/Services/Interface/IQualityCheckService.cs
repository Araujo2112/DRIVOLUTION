using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface;

public interface IQualityCheckService
{
    Task<IEnumerable<QualityCheckDTO>> GetByProduct(int productId);
    Task<QualityCheckDTO?> GetById(int id);
    Task<QualityCheckDTO> Create(CreateQualityCheckDTO dto);
}