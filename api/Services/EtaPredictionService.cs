using System.Net.Http.Json;
using System.Text.Json.Serialization;
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
    private readonly IPhaseTimeWeightCalculator _weightCalculator;
    private readonly HttpClient _httpClient;
    private readonly string _mlServiceUrl;

    public EtaPredictionService(
        IProductRepository productRepo,
        IProductConfigRepository productConfigRepo,
        IProductPhaseRepository productPhaseRepo,
        IPhaseSequenceRepository phaseSequenceRepo,
        IPhaseTimeCoefficientRepository coefficientRepo,
        IPhaseTimeWeightCalculator weightCalculator,
        IConfiguration configuration)
    {
        _productRepo = productRepo;
        _productConfigRepo = productConfigRepo;
        _productPhaseRepo = productPhaseRepo;
        _phaseSequenceRepo = phaseSequenceRepo;
        _coefficientRepo = coefficientRepo;
        _weightCalculator = weightCalculator;

        _mlServiceUrl = configuration["ML_SERVICE_URL"] ?? "http://ml-service:8000";
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(3)
        };
    }

    public async Task<DateTime?> PredictCurrentPhaseFinish(int productId)
{
    var result = await PredictCurrentPhaseDurationSecondsInternal(productId);
    if (result == null) return null;

    var currentPhase = await _productPhaseRepo.GetCurrentByProduct(productId);
    if (currentPhase == null) return null;

    var predictedFinish = currentPhase.DatetimeIni.AddSeconds((double)result.Value.Seconds);

    if (predictedFinish < DateTime.UtcNow)
        return DateTime.UtcNow;

    return predictedFinish;
}

    public async Task<PhaseDurationPredictionDTO?> PredictCurrentPhaseDurationSeconds(int productId)
    {
        var result = await PredictCurrentPhaseDurationSecondsInternal(productId);
        if (result == null) return null;

        return new PhaseDurationPredictionDTO
        {
            Seconds = (int)result.Value.Seconds,
            IsMlPrediction = result.Value.IsMl
        };
    }

    private async Task<(decimal Seconds, bool IsMl)?> PredictCurrentPhaseDurationSecondsInternal(int productId)
    {
        var product = await _productRepo.GetById(productId);
        if (product == null) return null;

        var currentPhase = await _productPhaseRepo.GetCurrentByProduct(productId);
        if (currentPhase == null) return null;

        var selectedOptionIds = (await _productConfigRepo.GetByProduct(productId))
            .Select(pc => pc.ConfigOptionId)
            .ToHashSet();

        var coefficients = (await _coefficientRepo.GetAll()).ToList();

        var lineId = currentPhase.Workstation.ProductionLineId;
        var fallbackSeconds = currentPhase.ManufacturingPhase.EstimatedDuration ?? 1800;

        return await PredictPhaseDurationSeconds(
            currentPhase.ManufacturingPhaseId,
            product.ModelId,
            selectedOptionIds,
            lineId,
            coefficients,
            fallbackSeconds
        );
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

        if (currentPhase == null && product.ProductionDate != null)
        {
            return new EtaResultDTO(
                product.Id,
                product.SerialNumber ?? "",
                product.ProductionDate.Value,
                "Concluído",
                0,
                trainedAt
            );
        }

        List<PhaseSequenceModel> remainingPhases;
        decimal elapsedInCurrentSeconds = 0;
        int? lineId = null;
        string currentPhaseName;

        if (currentPhase == null)
        {
            remainingPhases = sequence;
            currentPhaseName = "Não iniciado";
        }
        else
        {
            var currentIndex = sequence.FindIndex(ps =>
                ps.ManufacturingPhaseId == currentPhase.ManufacturingPhaseId);

            if (currentIndex == -1) return null;

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

            var prediction = await PredictPhaseDurationSeconds(
                phaseSeq.ManufacturingPhaseId,
                product.ModelId,
                selectedOptionIds,
                lineId,
                coefficients,
                fallbackSeconds
            );

            decimal remainingForThisPhase = i == 0 && currentPhase != null
                ? prediction.Seconds - elapsedInCurrentSeconds
                : prediction.Seconds;

            if (remainingForThisPhase < 0)
                remainingForThisPhase = 0;

            totalRemainingSeconds += remainingForThisPhase;
        }

        var eta = now.AddSeconds((double)totalRemainingSeconds);

        return new EtaResultDTO(
            product.Id,
            product.SerialNumber ?? "",
            eta,
            currentPhaseName,
            (int)totalRemainingSeconds,
            trainedAt
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

    public async Task<Dictionary<int, PhaseDurationPredictionDTO>> PredictCurrentPhaseDurationsForWip(
        IReadOnlyList<WipItemDTO> items)
    {
        if (items.Count == 0)
            return new Dictionary<int, PhaseDurationPredictionDTO>();

        var productIds = items.Select(i => i.ProductId).ToList();

        var coefficients = (await _coefficientRepo.GetAll()).ToList();

        var allConfigs = await _productConfigRepo.GetByProducts(productIds);
        var configsByProduct = allConfigs
            .GroupBy(pc => pc.ProductId)
            .ToDictionary(g => g.Key, g => g.Select(pc => pc.ConfigOptionId).ToHashSet());

        var openPhases = await _productPhaseRepo.GetCurrentByProducts(productIds);
        var phasesByProduct = openPhases.ToDictionary(pp => pp.ProductId);

        var result = new Dictionary<int, PhaseDurationPredictionDTO>();

        foreach (var item in items)
        {
            if (!phasesByProduct.TryGetValue(item.ProductId, out var currentPhase))
                continue;

            var selectedOptionIds = configsByProduct.TryGetValue(item.ProductId, out var opts)
                ? opts
                : new HashSet<int>();

            var lineId = currentPhase.Workstation.ProductionLineId;
            var fallbackSeconds = currentPhase.ManufacturingPhase.EstimatedDuration ?? 1800;

            var prediction = await PredictPhaseDurationSeconds(
                currentPhase.ManufacturingPhaseId,
                item.ModelId,
                selectedOptionIds,
                lineId,
                coefficients,
                fallbackSeconds
            );

            result[item.ProductId] = new PhaseDurationPredictionDTO
            {
                Seconds = (int)prediction.Seconds,
                IsMlPrediction = prediction.IsMl
            };
        }

        return result;
    }

    private async Task<(decimal Seconds, bool IsMl)> PredictPhaseDurationSeconds(
        int phaseId,
        int modelId,
        HashSet<int> optionIds,
        int? lineId,
        List<PhaseTimeCoefficientModel> coefficients,
        int fallbackSeconds)
    {
        var mlPrediction = await TryPredictWithMlService(
            phaseId,
            modelId,
            optionIds,
            lineId
        );

        if (mlPrediction.HasValue && mlPrediction.Value > 0)
        {
            return (mlPrediction.Value, true);
        }

        var seconds = _weightCalculator.PredictPhaseDurationSeconds(
            phaseId,
            modelId,
            optionIds,
            lineId,
            coefficients,
            fallbackSeconds
        );

        var isMl = _weightCalculator.HasTrainedIntercept(phaseId, coefficients);

        return (seconds, isMl);
    }

    private async Task<decimal?> TryPredictWithMlService(
        int phaseId,
        int modelId,
        HashSet<int> optionIds,
        int? lineId)
    {
        try
        {
            var payload = new MlPredictionRequest(
    phaseId,
    modelId,
    optionIds.ToList(),
    lineId,
    1800
);

            var response = await _httpClient.PostAsJsonAsync(
                $"{_mlServiceUrl}/predict",
                payload
            );

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadFromJsonAsync<MlPredictionResponse>();

            if (result == null || result.PredictedSeconds <= 0)
                return null;

            return result.PredictedSeconds;
        }
        catch
        {
            return null;
        }
    }

    private sealed record MlPredictionRequest(
    [property: JsonPropertyName("phase_id")] int PhaseId,
    [property: JsonPropertyName("model_id")] int ModelId,
    [property: JsonPropertyName("option_ids")] List<int> OptionIds,
    [property: JsonPropertyName("line_id")] int? LineId,
    [property: JsonPropertyName("fallback_seconds")] int FallbackSeconds
);

private sealed record MlPredictionResponse(
    [property: JsonPropertyName("predicted_seconds")] decimal PredictedSeconds,
    [property: JsonPropertyName("is_ml_prediction")] bool IsMlPrediction
);
}