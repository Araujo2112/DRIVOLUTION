using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IWipDashboardService
{
    Task<WipDashboardResultDTO> GetWipDashboardAsync();
}