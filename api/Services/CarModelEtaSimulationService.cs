using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class CarModelEtaSimulationService : ICarModelEtaSimulationService
{
    private readonly ICarModelRepository _carModelRepo;
    private readonly IPhaseSequenceRepository _phaseSequenceRepo;
    private readonly IPhaseTimeCoefficientRepository _coefficientRepo;
    private readonly IPhaseTimeWeightCalculator _weightCalculator;

    public CarModelEtaSimulationService(
        ICarModelRepository carModelRepo,
        IPhaseSequenceRepository phaseSequenceRepo,
        IPhaseTimeCoefficientRepository coefficientRepo,
        IPhaseTimeWeightCalculator weightCalculator)
    {
        _carModelRepo = carModelRepo;
        _phaseSequenceRepo = phaseSequenceRepo;
        _coefficientRepo = coefficientRepo;
        _weightCalculator = weightCalculator;
    }

    public async Task<EtaSimulationResult<EtaSimulationResultDTO>> Simulate(
        int modelId, IEnumerable<int> configOptionIds)
    {
        var model = await _carModelRepo.GetById(modelId);
        if (model == null)
            return EtaSimulationResult<EtaSimulationResultDTO>.Fail(
                EtaSimulationErrorCode.ModelNotFound, $"Modelo {modelId} não encontrado.");

        var requestedOptionIds = configOptionIds.Distinct().ToList();

        // Validação de pertença: cada optionId tem de pertencer a uma Config deste
        // modelo. Sem isto, um optionId de outro modelo seria simplesmente ignorado
        // (peso 0) sem aviso nenhum, dando um resultado plausível mas errado.
        // Carregar configs e opções com Include evita N+1 queries (uma só query,
        // filtragem feita em memória).
        var modelConfigs = (await _carModelRepo.GetConfigsWithOptions(modelId)).ToList();
        var validOptionIds = modelConfigs
            .SelectMany(c => c.ConfigOptions)
            .Select(o => o.Id)
            .ToHashSet();

        var invalidOptionIds = requestedOptionIds.Where(id => !validOptionIds.Contains(id)).ToList();
        if (invalidOptionIds.Count > 0)
        {
            return EtaSimulationResult<EtaSimulationResultDTO>.Fail(
                EtaSimulationErrorCode.OptionNotFoundForModel,
                $"As opções [{string.Join(", ", invalidOptionIds)}] não pertencem ao modelo {modelId}.");
        }

        var sequence = (await _phaseSequenceRepo.GetByModel(modelId)).ToList();
        var coefficients = (await _coefficientRepo.GetAll()).ToList();
        var trainedAt = await _coefficientRepo.GetLastTrainedAt();

        // O treino (train_phase_time_model.py) escolhe um modelo de referência
        // ("baseline") que nunca recebe peso próprio — o comportamento dele já
        // está embutido no intercepto de cada fase. Por isso "este modelo não
        // tem peso próprio" não é o sinal certo de "faltam dados": um modelo
        // pode ser exatamente o baseline e ter histórico de produção completo.
        // O sinal correto é simplesmente "o treino já correu" (trainedAt existe)
        // — se nunca correu, a previsão cai toda para o valor estático
        // (EstimatedDuration), e é isso que vale a pena avisar.
        var estimateIsTrained = trainedAt != null;

        var phaseResults = new List<PhaseEtaSimulationDTO>();
        int totalSeconds = 0;

        // Sem lineId: a simulação é hipotética e não está associada a nenhuma
        // linha de produção real, por isso lineId é sempre null aqui — a fórmula
        // (PhaseTimeWeightCalculator) já trata lineId nulo como "sem peso de linha".
        foreach (var phaseSeq in sequence)
        {
            var fallbackSeconds = phaseSeq.ManufacturingPhase.EstimatedDuration
                ?? PhaseTimeWeightCalculator.MinDurationSecondsPerPhase;

            var predictedSeconds = (int)_weightCalculator.PredictPhaseDurationSeconds(
                phaseSeq.ManufacturingPhaseId, modelId, requestedOptionIds, lineId: null,
                coefficients, fallbackSeconds);

            phaseResults.Add(new PhaseEtaSimulationDTO(
                phaseSeq.ManufacturingPhaseId, phaseSeq.ManufacturingPhase.Name.Trim(),
                phaseSeq.Order, predictedSeconds));

            totalSeconds += predictedSeconds;
        }

        var result = new EtaSimulationResultDTO(
            modelId, model.Name, requestedOptionIds, phaseResults, totalSeconds,
            estimateIsTrained, trainedAt);

        return EtaSimulationResult<EtaSimulationResultDTO>.Ok(result);
    }
}