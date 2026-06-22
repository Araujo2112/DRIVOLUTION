using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface;

public interface IWipDashboardService
{
    Task<WipDashboardResultDTO> GetWipDashboardAsync();
}