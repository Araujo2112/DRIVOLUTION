using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ProductionLineStatusService : IProductionLineStatusService
{
    private readonly IProductionLineStatusRepository _repository;
    private readonly IEtaPredictionService _etaService;

    public ProductionLineStatusService(
        IProductionLineStatusRepository repository,
        IEtaPredictionService etaService)
    {
        _repository = repository;
        _etaService = etaService;
    }

    public async Task<List<ProductionLineStatusDTO>> GetProductionLineStatusAsync()
    {
        var status = await _repository.GetStatusAsync();

        // EstimatedFinish passa a ser o ETA completo do carro (todas as fases
        // que faltam até saír da linha), não só o tempo desta fase — uma linha
        // representa o percurso todo (todas as workstations em sequência),
        // não uma estação isolada.
        foreach (var row in status)
        {
            if (row.ProductId == null) continue;

            var eta = await _etaService.PredictForProduct(row.ProductId.Value);
            if (eta != null)
                row.EstimatedFinish = eta.EstimatedCompletion;
        }

        return status;
    }
}