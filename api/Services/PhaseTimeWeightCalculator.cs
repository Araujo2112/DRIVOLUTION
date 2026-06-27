using Drivolution.Models;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

/// <inheritdoc />
public class PhaseTimeWeightCalculator : IPhaseTimeWeightCalculator
{
    // Piso de segurança para a previsão estática de duração de uma fase
    // (regressão de coeficientes). Nunca prevê menos do que isto como duração
    // total de uma fase. NÃO se aplica ao "remaining" de uma fase já em curso:
    // esse pode e deve ir a negativo quando a fase está atrasada.
    // Constante pública para que CarModelEtaSimulationService a use no fallback
    // quando EstimatedDuration da fase é null.
    public const int MinDurationSecondsPerPhase = 60;

    /// <inheritdoc />
    public decimal PredictPhaseDurationSeconds(
        int phaseId,
        int modelId,
        IEnumerable<int> selectedOptionIds,
        int? lineId,
        List<PhaseTimeCoefficientModel> coefficients,
        int fallbackSeconds)
    {
        var intercept = coefficients.FirstOrDefault(c =>
            c.ManufacturingPhaseId == phaseId &&
            c.ConfigOptionId == null && c.ProductionLineId == null && c.ModelId == null
        );

        // Sem coeficientes treinados para esta fase ainda (ex: fase nova, nunca treinada) —
        // cai para a estimativa estática, exatamente como o card pede ("cold start").
        if (intercept == null)
            return fallbackSeconds;

        decimal total = intercept.WeightSeconds;

        var modelWeight = coefficients.FirstOrDefault(c =>
            c.ManufacturingPhaseId == phaseId && c.ModelId == modelId &&
            c.ConfigOptionId == null && c.ProductionLineId == null
        )?.WeightSeconds ?? 0;
        total += modelWeight;

        if (lineId.HasValue)
        {
            var lineWeight = coefficients.FirstOrDefault(c =>
                c.ManufacturingPhaseId == phaseId && c.ProductionLineId == lineId &&
                c.ConfigOptionId == null && c.ModelId == null
            )?.WeightSeconds ?? 0;
            total += lineWeight;
        }

        foreach (var optionId in selectedOptionIds)
        {
            var optionWeight = coefficients.FirstOrDefault(c =>
                c.ManufacturingPhaseId == phaseId && c.ConfigOptionId == optionId
            )?.WeightSeconds ?? 0;
            total += optionWeight;
        }

        return total < MinDurationSecondsPerPhase ? MinDurationSecondsPerPhase : total;
    }

    public bool HasTrainedIntercept(int phaseId, List<PhaseTimeCoefficientModel> coefficients)
    {
        return coefficients.Any(c =>
            c.ManufacturingPhaseId == phaseId &&
            c.ConfigOptionId == null && c.ProductionLineId == null && c.ModelId == null
        );
    }
}