using ApiTexPact.DTO;

namespace ApiTexPact.Repository.Interface;

public interface IWipDashboardRepository
{
    Task<List<WipItemDTO>> GetInProgressAsync();
    Task<List<WaitingItemDTO>> GetWaitingAsync();
    Task<int> GetCompletedCountAsync();
}