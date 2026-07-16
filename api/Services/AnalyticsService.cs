using Drivolution.DTO.Analytics;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável por fornecer os indicadores analíticos da plataforma
public class AnalyticsService : IAnalyticsService
{
    // Repository que obtém os dados analíticos da base de dados
    private readonly IAnalyticsRepository _repository;

    // O ASP.NET injeta automaticamente o repository
    public AnalyticsService(IAnalyticsRepository repository)
    {
        _repository = repository;
    }

    // Devolve a duração média de cada fase de fabrico
    public Task<List<PhaseDurationDTO>> GetPhaseDurationsAsync()
    {
        return _repository.GetPhaseDurationsAsync();
    }

    // Devolve a taxa de retrabalho (rework) de cada fase
    public Task<List<ReworkRateDTO>> GetReworkRatesAsync()
    {
        return _repository.GetReworkRatesAsync();
    }

    // Devolve o throughput da produção,
    // ou seja, a quantidade de produtos concluídos por período
    public Task<List<ThroughputDTO>> GetThroughputAsync()
    {
        return _repository.GetThroughputAsync();
    }
}