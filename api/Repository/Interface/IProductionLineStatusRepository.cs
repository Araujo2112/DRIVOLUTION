using Drivolution.DTO;

namespace Drivolution.Repository.Interface;

public interface IProductionLineStatusRepository
{
    Task<List<ProductionLineStatusDTO>> GetStatusAsync();
}