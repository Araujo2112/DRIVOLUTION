using Drivolution.DTO.Analytics;

namespace Drivolution.Repository.Interface;

public interface IAnalyticsRepository
{
    Task<List<PhaseDurationDTO>> GetPhaseDurationsAsync();
    Task<List<ReworkRateDTO>> GetReworkRatesAsync();
    Task<List<ThroughputDTO>> GetThroughputAsync();
}