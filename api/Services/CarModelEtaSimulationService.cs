using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável por simular o tempo total de produção
// de um modelo de veículo antes de existir um produto real
public class CarModelEtaSimulationService : ICarModelEtaSimulationService
{
    // Repository usado para consultar os modelos e as suas configurações
    private readonly ICarModelRepository _carModelRepo;

    // Repository usado para obter a sequência de fases do modelo
    private readonly IPhaseSequenceRepository _phaseSequenceRepo;

    // Repository usado para obter os coeficientes aprendidos pelo modelo
    private readonly IPhaseTimeCoefficientRepository _coefficientRepo;

    // Componente responsável por calcular a duração prevista de cada fase
    private readonly IPhaseTimeWeightCalculator _weightCalculator;

    // O ASP.NET injeta automaticamente os repositories e serviços necessários
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

    // Simula o tempo total de fabrico para um modelo e um conjunto
    // de opções de configuração escolhidas
    public async Task<EtaSimulationResult<EtaSimulationResultDTO>> Simulate(
        int modelId, IEnumerable<int> configOptionIds)
    {
        // Procura o modelo de veículo
        var model = await _carModelRepo.GetById(modelId);

        // Se não existir, devolve um resultado de erro
        if (model == null)
            return EtaSimulationResult<EtaSimulationResultDTO>.Fail(
                EtaSimulationErrorCode.ModelNotFound,
                $"Modelo {modelId} não encontrado.");

        // Remove opções repetidas e transforma o resultado numa lista
        var requestedOptionIds = configOptionIds
            .Distinct()
            .ToList();

        // Validação de pertença: cada optionId tem de pertencer a uma Config deste
        // modelo. Sem isto, um optionId de outro modelo seria simplesmente ignorado
        // (peso 0) sem aviso nenhum, dando um resultado plausível mas errado.
        // Carregar configs e opções com Include evita N+1 queries (uma só query,
        // filtragem feita em memória).

        // Obtém todas as configurações do modelo, incluindo as suas opções
        var modelConfigs = (
            await _carModelRepo.GetConfigsWithOptions(modelId)
        ).ToList();

        // Cria um conjunto com todos os IDs de opções válidas para este modelo
        var validOptionIds = modelConfigs
            .SelectMany(c => c.ConfigOptions)
            .Select(o => o.Id)
            .ToHashSet();

        // Procura opções recebidas que não pertencem ao modelo
        var invalidOptionIds = requestedOptionIds
            .Where(id => !validOptionIds.Contains(id))
            .ToList();

        // Se existirem opções inválidas, termina a simulação com erro
        if (invalidOptionIds.Count > 0)
        {
            return EtaSimulationResult<EtaSimulationResultDTO>.Fail(
                EtaSimulationErrorCode.OptionNotFoundForModel,
                $"As opções [{string.Join(", ", invalidOptionIds)}] não pertencem ao modelo {modelId}.");
        }

        // Obtém a sequência de fases do modelo
        var sequence = (
            await _phaseSequenceRepo.GetByModel(modelId)
        ).ToList();

        // Obtém todos os coeficientes usados nos cálculos
        var coefficients = (
            await _coefficientRepo.GetAll()
        ).ToList();

        // Obtém a data do último treino do modelo
        var trainedAt = await _coefficientRepo.GetLastTrainedAt();

        // O treino (train_phase_time_model.py) escolhe um modelo de referência
        // ("baseline") que nunca recebe peso próprio — o comportamento dele já
        // está embutido no intercepto de cada fase. Por isso "este modelo não
        // tem peso próprio" não é o sinal certo de "faltam dados": um modelo
        // pode ser exatamente o baseline e ter histórico de produção completo.
        // O sinal correto é simplesmente "o treino já correu" (trainedAt existe)
        // — se nunca correu, a previsão cai toda para o valor estático
        // (EstimatedDuration), e é isso que vale a pena avisar.

        // Indica se já existe um treino anterior
        var estimateIsTrained = trainedAt != null;

        // Lista onde serão guardadas as previsões de cada fase
        var phaseResults = new List<PhaseEtaSimulationDTO>();

        // Acumulador do tempo total previsto
        int totalSeconds = 0;

        // Sem lineId: a simulação é hipotética e não está associada a nenhuma
        // linha de produção real, por isso lineId é sempre null aqui — a fórmula
        // (PhaseTimeWeightCalculator) já trata lineId nulo como "sem peso de linha".
        foreach (var phaseSeq in sequence)
        {
            // Usa a duração estimada da fase como fallback.
            // Se não existir, usa a duração mínima definida pelo sistema.
            var fallbackSeconds =
                phaseSeq.ManufacturingPhase.EstimatedDuration
                ?? PhaseTimeWeightCalculator.MinDurationSecondsPerPhase;

            // Calcula a duração prevista da fase com base:
            // - na fase;
            // - no modelo;
            // - nas opções escolhidas;
            // - nos coeficientes treinados;
            // - e no valor de fallback.
            var predictedSeconds =
                (int)_weightCalculator.PredictPhaseDurationSeconds(
                    phaseSeq.ManufacturingPhaseId,
                    modelId,
                    requestedOptionIds,
                    lineId: null,
                    coefficients,
                    fallbackSeconds
                );

            // Guarda o resultado desta fase
            phaseResults.Add(
                new PhaseEtaSimulationDTO(
                    phaseSeq.ManufacturingPhaseId,
                    phaseSeq.ManufacturingPhase.Name.Trim(),
                    phaseSeq.Order,
                    predictedSeconds
                )
            );

            // Soma o tempo desta fase ao total
            totalSeconds += predictedSeconds;
        }

        // Constrói o resultado final da simulação
        var result = new EtaSimulationResultDTO(
            modelId,
            model.Name,
            requestedOptionIds,
            phaseResults,
            totalSeconds,
            estimateIsTrained,
            trainedAt
        );

        // Devolve o resultado como operação bem-sucedida
        return EtaSimulationResult<EtaSimulationResultDTO>.Ok(result);
    }
}