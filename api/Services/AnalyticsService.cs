using Drivolution.DTO.Analytics;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repository;

    public AnalyticsService(IAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<List<PhaseDurationDTO>> GetPhaseDurationsAsync()
    {
        return _repository.GetPhaseDurationsAsync();
    }

    public Task<List<ReworkRateDTO>> GetReworkRatesAsync()
    {
        return _repository.GetReworkRatesAsync();
    }

    public Task<List<ThroughputDTO>> GetThroughputAsync()
    {
        return _repository.GetThroughputAsync();
    }
}