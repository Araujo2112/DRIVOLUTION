using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class EtaPredictionService : IEtaPredictionService
{
    private readonly IProductRepository _productRepo;
    private readonly IProductConfigRepository _productConfigRepo;
    private readonly IProductPhaseRepository _productPhaseRepo;
    private readonly IPhaseSequenceRepository _phaseSequenceRepo;
    private readonly IPhaseTimeCoefficientRepository _coefficientRepo;

    // Piso de segurança: nunca prever menos do que isto para uma fase em curso,
    // mesmo que já tenha passado o tempo previsto (evita ETAs no passado).
    private const int MinRemainingSecondsPerPhase = 60;

    public EtaPredictionService(
        IProductRepository productRepo,
        IProductConfigRepository productConfigRepo,
        IProductPhaseRepository productPhaseRepo,
        IPhaseSequenceRepository phaseSequenceRepo,
        IPhaseTimeCoefficientRepository coefficientRepo)
    {
        _productRepo = productRepo;
        _productConfigRepo = productConfigRepo;
        _productPhaseRepo = productPhaseRepo;
        _phaseSequenceRepo = phaseSequenceRepo;
        _coefficientRepo = coefficientRepo;
    }

    public async Task<DateTime?> PredictCurrentPhaseFinish(int productId)
    {
        var product = await _productRepo.GetById(productId);
        if (product == null) return null;

        var currentPhase = await _productPhaseRepo.GetCurrentByProduct(productId);
        if (currentPhase == null) return null; // sem fase aberta — nada a prever aqui

        var selectedOptionIds = (await _productConfigRepo.GetByProduct(productId))
            .Select(pc => pc.ConfigOptionId)
            .ToHashSet();

        var coefficients = (await _coefficientRepo.GetAll()).ToList();
        var lineId = currentPhase.Workstation.ProductionLineId;
        var fallbackSeconds = currentPhase.ManufacturingPhase.EstimatedDuration ?? 1800;

        var predictedFullSeconds = PredictPhaseDurationSeconds(
            currentPhase.ManufacturingPhaseId, product.ModelId, selectedOptionIds, lineId,
            coefficients, fallbackSeconds
        );

        return currentPhase.DatetimeIni.AddSeconds((double)predictedFullSeconds);
    }

    public async Task<EtaResultDTO?> PredictForProduct(int productId)
    {
        var product = await _productRepo.GetById(productId);
        if (product == null) return null;

        var selectedOptionIds = (await _productConfigRepo.GetByProduct(productId))
            .Select(pc => pc.ConfigOptionId)
            .ToHashSet();

        var currentPhase = await _productPhaseRepo.GetCurrentByProduct(productId);
        var sequence = (await _phaseSequenceRepo.GetByModel(product.ModelId)).ToList();
        var coefficients = (await _coefficientRepo.GetAll()).ToList();
        var trainedAt = await _coefficientRepo.GetLastTrainedAt();

        var now = DateTime.UtcNow;

        // Caso 1: produto já concluiu todas as fases.
        if (currentPhase == null && product.ProductionDate != null)
        {
            return new EtaResultDTO(
                product.Id, product.SerialNumber ?? "", product.ProductionDate.Value,
                "Concluído", 0, trainedAt
            );
        }

        List<PhaseSequenceModel> remainingPhases;
        decimal elapsedInCurrentSeconds = 0;
        int? lineId = null;
        string currentPhaseName;

        if (currentPhase == null)
        {
            // Caso 2: ainda não entrou em nenhuma fase — prevê a sequência toda a partir de agora.
            remainingPhases = sequence;
            currentPhaseName = "Não iniciado";
        }
        else
        {
            var currentIndex = sequence.FindIndex(ps => ps.ManufacturingPhaseId == currentPhase.ManufacturingPhaseId);
            if (currentIndex == -1)
            {
                // Inconsistência de dados: a fase atual não pertence à sequência do modelo.
                return null;
            }

            remainingPhases = sequence.Skip(currentIndex).ToList();
            elapsedInCurrentSeconds = (decimal)(now - currentPhase.DatetimeIni).TotalSeconds;
            if (elapsedInCurrentSeconds < 0) elapsedInCurrentSeconds = 0;

            lineId = currentPhase.Workstation.ProductionLineId;
            currentPhaseName = currentPhase.ManufacturingPhase.Name.Trim();
        }

        decimal totalRemainingSeconds = 0;

        for (int i = 0; i < remainingPhases.Count; i++)
        {
            var phaseSeq = remainingPhases[i];
            var fallbackSeconds = phaseSeq.ManufacturingPhase.EstimatedDuration ?? 1800;

            var predictedFullSeconds = PredictPhaseDurationSeconds(
                phaseSeq.ManufacturingPhaseId, product.ModelId, selectedOptionIds, lineId,
                coefficients, fallbackSeconds
            );

            decimal remainingForThisPhase;
            if (i == 0 && currentPhase != null)
            {
                remainingForThisPhase = predictedFullSeconds - elapsedInCurrentSeconds;
                if (remainingForThisPhase < MinRemainingSecondsPerPhase)
                    remainingForThisPhase = MinRemainingSecondsPerPhase;
            }
            else
            {
                remainingForThisPhase = predictedFullSeconds;
            }

            totalRemainingSeconds += remainingForThisPhase;
        }

        var eta = now.AddSeconds((double)totalRemainingSeconds);

        return new EtaResultDTO(
            product.Id, product.SerialNumber ?? "", eta,
            currentPhaseName, (int)totalRemainingSeconds, trainedAt
        );
    }

    public async Task<List<EtaResultDTO>> PredictForProductionLine(int productionLineId)
    {
        var openPhases = await _productPhaseRepo.GetAllOpenByProductionLine(productionLineId);

        var results = new List<EtaResultDTO>();
        foreach (var phase in openPhases)
        {
            var result = await PredictForProduct(phase.ProductId);
            if (result != null) results.Add(result);
        }

        return results;
    }

    private decimal PredictPhaseDurationSeconds(
        int phaseId, int modelId, HashSet<int> selectedOptionIds, int? lineId,
        List<PhaseTimeCoefficientModel> coefficients, int fallbackSeconds)
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

        return total < MinRemainingSecondsPerPhase ? MinRemainingSecondsPerPhase : total;
    }
}