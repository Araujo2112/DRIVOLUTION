using Drivolution.Models;

namespace Drivolution.Services.Interface;


// Calcula a duração prevista de uma fase de fabrico com base nos coeficientes treinados pelo modelo de ML (Ridge). 
// Partilhado entre EtaPredictionService (previsão de produtos reais) e CarModelEtaSimulationService (simulação hipotética sem produto real na BD).

public interface IPhaseTimeWeightCalculator
{
    // Devolve a duração prevista em segundos para uma fase, dado o modelo, as opções de configuração selecionadas 
    // e (opcionalmente) a linha de produção. Se não existirem coeficientes treinados para a fase (cold start), devolve

    decimal PredictPhaseDurationSeconds(
        int phaseId,
        int modelId,
        IEnumerable<int> selectedOptionIds,
        int? lineId,
        List<PhaseTimeCoefficientModel> coefficients,
        int fallbackSeconds);

    //Indica se o modelo de ML já foi treinado para a fase indicada, ou seja, se existe um coeficiente interceto gravado na BD. 
    // Quando false, a previsão usa o valor estático (fallback).
    bool HasTrainedIntercept(int phaseId, List<PhaseTimeCoefficientModel> coefficients);
}