using Drivolution.Models;

namespace Drivolution.Services.Interface;

// Calcula a duração estimada de uma fase de fabrico a partir dos pesos
// treinados (intercepto + modelo + linha de produção + opções de configuração).
// Sem estado e sem acesso à base de dados — recebe os coeficientes já
// carregados. Partilhada entre a previsão de produtos reais em produção e a
// simulação de carros hipotéticos, para garantir a mesma fórmula nos dois casos.
public interface IPhaseTimeWeightCalculator
{
    decimal PredictPhaseDurationSeconds(
        int phaseId,
        int? modelId,
        IEnumerable<int> selectedOptionIds,
        int? lineId,
        IReadOnlyCollection<PhaseTimeCoefficientModel> coefficients,
        int fallbackSeconds);
}