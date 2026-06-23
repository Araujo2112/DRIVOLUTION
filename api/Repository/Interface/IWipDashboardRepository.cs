using Drivolution.DTO;

namespace Drivolution.Repository.Interface;

public interface IWipDashboardRepository
{
    Task<List<WipItemDTO>> GetInProgressAsync();
    Task<List<WaitingItemDTO>> GetWaitingAsync();
    Task<int> GetCompletedCountAsync();
}