using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ProductionLineStatusService : IProductionLineStatusService
{
    // Repository responsável por obter o estado das linhas de produção
    private readonly IProductionLineStatusRepository _repository;
    // Service responsável por calcular as ETAs dos produtos
    private readonly IEtaPredictionService _etaService;

    // O ASP.NET injeta automaticamente o repository e o service
    public ProductionLineStatusService(
        IProductionLineStatusRepository repository,
        IEtaPredictionService etaService)
    {
        _repository = repository;
        _etaService = etaService;
    }

    // Devolve o estado atual de todas as linhas de produção
    public async Task<List<ProductionLineStatusDTO>> GetProductionLineStatusAsync()
    {
        // Obtém o estado atual das linhas através do repository
        var status = await _repository.GetStatusAsync();

        // EstimatedFinish passa a ser o ETA completo do carro (todas as fases
        // que faltam até saír da linha), não só o tempo desta fase — uma linha
        // representa o percurso todo (todas as workstations em sequência),
        // não uma estação isolada.
        foreach (var row in status)
        {
            // Se não existir nenhum produto nessa linha, passa à seguinte
            if (row.ProductId == null) continue;

            // Calcula a ETA completa do produto
            var eta = await _etaService.PredictForProduct(row.ProductId.Value);
            // Se foi possível calcular a previsão,
            // guarda a data estimada de conclusão
            if (eta != null)
                row.EstimatedFinish = eta.EstimatedCompletion;
        }

        // Devolve a lista já com as ETAs calculadas
        return status;
    }
}