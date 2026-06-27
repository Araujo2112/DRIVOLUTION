using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IEtaPredictionService
{
    Task<EtaResultDTO?> PredictForProduct(int productId);
    Task<DateTime?> PredictCurrentPhaseFinish(int productId);
    Task<PhaseDurationPredictionDTO?> PredictCurrentPhaseDurationSeconds(int productId);
    Task<List<EtaResultDTO>> PredictForProductionLine(int productionLineId);
    Task<Dictionary<int, PhaseDurationPredictionDTO>> PredictCurrentPhaseDurationsForWip(
        IReadOnlyList<WipItemDTO> items);
}