using Drivolution.Models;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

/// <inheritdoc />
// Service responsável por calcular a duração prevista de uma fase
// com base nos coeficientes aprendidos pelo modelo de Machine Learning
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
    // Calcula a duração prevista de uma fase
    public decimal PredictPhaseDurationSeconds(
        int phaseId,
        int modelId,
        IEnumerable<int> selectedOptionIds,
        int? lineId,
        List<PhaseTimeCoefficientModel> coefficients,
        int fallbackSeconds)
    {
        // Procura o intercepto da fase.
        // O intercepto representa a duração base da fase,
        // antes de aplicar quaisquer ajustes.
        var intercept = coefficients.FirstOrDefault(c =>
            c.ManufacturingPhaseId == phaseId &&
            c.ConfigOptionId == null &&
            c.ProductionLineId == null &&
            c.ModelId == null
        );

        // Sem coeficientes treinados para esta fase ainda (ex: fase nova, nunca treinada) —
        // cai para a estimativa estática, exatamente como o card pede ("cold start").
        if (intercept == null)
            return fallbackSeconds;

        // Começa pela duração base (intercepto)
        decimal total = intercept.WeightSeconds;

        // Procura o peso específico do modelo de veículo
        var modelWeight = coefficients.FirstOrDefault(c =>
            c.ManufacturingPhaseId == phaseId &&
            c.ModelId == modelId &&
            c.ConfigOptionId == null &&
            c.ProductionLineId == null
        )?.WeightSeconds ?? 0;

        // Soma esse peso ao valor total
        total += modelWeight;

        // Se existir uma linha de produção, procura também o peso dessa linha
        if (lineId.HasValue)
        {
            var lineWeight = coefficients.FirstOrDefault(c =>
                c.ManufacturingPhaseId == phaseId &&
                c.ProductionLineId == lineId &&
                c.ConfigOptionId == null &&
                c.ModelId == null
            )?.WeightSeconds ?? 0;

            total += lineWeight;
        }

        // Para cada opção de configuração escolhida,
        // soma o respetivo peso aprendido pelo modelo
        foreach (var optionId in selectedOptionIds)
        {
            var optionWeight = coefficients.FirstOrDefault(c =>
                c.ManufacturingPhaseId == phaseId &&
                c.ConfigOptionId == optionId
            )?.WeightSeconds ?? 0;

            total += optionWeight;
        }

        // Nunca devolve uma duração inferior ao mínimo definido
        return total < MinDurationSecondsPerPhase
            ? MinDurationSecondsPerPhase
            : total;
    }

    // Verifica se já existe um intercepto treinado para uma determinada fase
    public bool HasTrainedIntercept(
        int phaseId,
        List<PhaseTimeCoefficientModel> coefficients)
    {
        return coefficients.Any(c =>
            c.ManufacturingPhaseId == phaseId &&
            c.ConfigOptionId == null &&
            c.ProductionLineId == null &&
            c.ModelId == null
        );
    }
}