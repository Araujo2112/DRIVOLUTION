using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IProductionLineStatusService
{
    Task<List<ProductionLineStatusDTO>> GetProductionLineStatusAsync();
}