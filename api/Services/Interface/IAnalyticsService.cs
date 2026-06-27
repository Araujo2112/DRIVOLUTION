using Drivolution.DTO.Analytics;

namespace Drivolution.Services.Interface;

public interface IAnalyticsService
{
    Task<List<PhaseDurationDTO>> GetPhaseDurationsAsync();
    Task<List<ReworkRateDTO>> GetReworkRatesAsync();
    Task<List<ThroughputDTO>> GetThroughputAsync();
}