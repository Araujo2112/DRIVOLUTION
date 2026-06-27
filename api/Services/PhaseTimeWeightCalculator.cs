using Drivolution.Models;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Calcula a duração estimada de uma fase de fabrico somando os pesos treinados
// guardados em phase_time_coefficient: um intercepto base por fase, mais um
// peso opcional por modelo de carro, por linha de produção e por cada opção de
// configuração selecionada. Não acede à base de dados — recebe a lista de
// coeficientes já carregada e só faz a soma. Usado tanto para prever o tempo
// restante de um produto real em produção como para simular o tempo de um
// carro hipotético antes de existir nenhum produto — para que as duas previsões
// usem sempre exatamente a mesma fórmula.
public class PhaseTimeWeightCalculator : IPhaseTimeWeightCalculator
{
    // Piso de segurança: a duração prevista de uma fase nunca é inferior a isto,
    // mesmo que a soma dos pesos dê um valor muito baixo ou negativo.
    public const int MinDurationSecondsPerPhase = 60;

    // Soma intercepto + peso do modelo (se indicado) + peso da linha de produção
    // (se indicada) + soma dos pesos de cada opção de configuração selecionada.
    // modelId == null significa "sem modelo conhecido" — não soma nenhum peso de
    // modelo (ex: ainda não foi escolhido um modelo na simulação).
    public decimal PredictPhaseDurationSeconds(
        int phaseId,
        int? modelId,
        IEnumerable<int> selectedOptionIds,
        int? lineId,
        IReadOnlyCollection<PhaseTimeCoefficientModel> coefficients,
        int fallbackSeconds)
    {
        var intercept = coefficients.FirstOrDefault(c =>
            c.ManufacturingPhaseId == phaseId &&
            c.ConfigOptionId == null && c.ProductionLineId == null && c.ModelId == null
        );

        // Sem coeficientes treinados para esta fase ainda (ex: fase nova, nunca
        // treinada) — cai para a estimativa estática ("cold start").
        if (intercept == null)
            return fallbackSeconds;

        decimal total = intercept.WeightSeconds;

        if (modelId.HasValue)
        {
            var modelWeight = coefficients.FirstOrDefault(c =>
                c.ManufacturingPhaseId == phaseId && c.ModelId == modelId &&
                c.ConfigOptionId == null && c.ProductionLineId == null
            )?.WeightSeconds ?? 0;
            total += modelWeight;
        }

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
}