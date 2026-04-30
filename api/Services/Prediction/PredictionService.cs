using ApiTexPact.Data;
using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Prediction;
using ApiTexPact.Services.ArrowFlightClient.Interfaces;
using ApiTexPact.Services.Prediction.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Services.Prediction;

public class PredictionService : IPredictionService
{
    private readonly ApplicationDbContext _context;
    private readonly IQuicPredictionClientService _quic;
    private readonly IPredictionRepository _repository;

    public PredictionService(IPredictionRepository repository,
        ApplicationDbContext context,
        IQuicPredictionClientService quic)
    {
        _repository = repository;
        _context = context;
        _quic = quic;
    }

    public async Task<PredictionDTO> CreatePredictionAsync(PredictionDTO dto)
    {
        var model = new PredictionModel
        {
            Model = dto.Model,
            LastDate = dto.LastDate,
            ModelVersion = dto.ModelVersion,
            ModelType = dto.ModelType
        };
        var created = await _repository.CreateAsync(model);
        return ToDTO(created);
    }

    public async Task<PredictionDTO> GetPredictionByIdAsync(int id)
    {
        var model = await _repository.GetByIdAsync(id);
        return model != null ? ToDTO(model) : null;
    }

    public async Task<List<PredictionDTO>> GetAllPredictionsAsync()
    {
        var models = await _repository.GetAllAsync();
        var dtos = new List<PredictionDTO>();
        foreach (var model in models)
            dtos.Add(ToDTO(model));
        return dtos;
    }

    public async Task<PredictionDTO> UpdatePredictionAsync(PredictionDTO dto)
    {
        var model = await _repository.GetByIdAsync(dto.Id);
        if (model == null) return null;

        model.Model = dto.Model;
        model.LastDate = dto.LastDate;
        model.ModelVersion = dto.ModelVersion;
        model.ModelType = dto.ModelType;

        var updated = await _repository.UpdateAsync(model);
        return ToDTO(updated);
    }

    public async Task<bool> DeletePredictionAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<TrainResultDto> TrainAsync()
    {
        // 1) obtém o último prediction (ou null)
        var all = await _repository.GetAllAsync();
        var existing = all.OrderByDescending(p => p.Id).FirstOrDefault();

        var cutoff = existing?.LastDate ?? DateTime.MinValue;
        var nextVersion = (existing?.ModelVersion ?? 0) + 1;
        var lastModel = existing?.Model ?? Array.Empty<byte>();

        // 2) busca apenas colunas primitivas via EF
        var rawPrimitives = await _context.ManufacturingOrderPhases
            .Where(ph => ph.DateTimeEnd > ph.DateTimeInit && ph.DateTimeEnd > cutoff)
            .Select(ph => new
            {
                ph.Quantity,
                ScheduleInit = ph.SheduleInit,
                LabelSeconds = (ph.DateTimeEnd - ph.DateTimeInit).TotalSeconds
            })
            .ToListAsync();

        if (!rawPrimitives.Any())
            throw new InvalidOperationException($"Sem dados novos desde {cutoff:O}");

        // 3) monta o dataset com dicionários em memória
        var dataset = rawPrimitives
            .Select(x =>
            {
                var dict = new Dictionary<string, object>
                {
                    ["Quantity"] = x.Quantity,
                    ["ScheduleInitTs"] = new DateTimeOffset(x.ScheduleInit).ToUnixTimeSeconds(),
                    ["label"] = x.LabelSeconds
                };
                return dict;
            })
            .ToList();

        // 4) envia para Python e recebe o novo modelo ONNX
        var newModel = await _quic.SendTrainDataAsync(
            lastModel,
            nextVersion,
            DateTime.UtcNow,
            "fasttree",
            dataset
        );

        // 5) persiste via repositório
        var dto = new PredictionDTO
        {
            Id = existing?.Id ?? 0,
            Model = newModel,
            LastDate = DateTime.UtcNow,
            ModelVersion = nextVersion,
            ModelType = "fasttree"
        };

        var saved = existing is null
            ? await CreatePredictionAsync(dto)
            : await UpdatePredictionAsync(dto);

        return new TrainResultDto
        {
            Message = "Treino concluído",
            Version = saved.ModelVersion,
            TrainedUntil = saved.LastDate
        };
    }

    public async Task<float[]> PredictAsync(List<SinglePredictRequest> features)
    {
        if (features == null || !features.Any())
            throw new ArgumentException("Passe pelo menos uma linha de features.", nameof(features));

        var latest = await _repository.GetLatestModelAsync();
        if (latest == null || latest.Model == null || latest.Model.Length == 0)
            throw new InvalidOperationException("Ainda não há modelo treinado.");

        var modelBytes = latest.Model;

        var dicts = features.Select(f => new Dictionary<string, object>
        {
            ["Quantity"] = f.Quantity,
            ["PhaseId"] = f.PhaseId,
            ["SectionId"] = f.SectionId,
            ["ProductId"] = f.ProductId,
            ["ClientId"] = f.ClientId,
            ["LotQty"] = f.LotQty,
            ["Hour"] = f.Hour,
            ["WeekDay"] = f.WeekDay
        }).ToList();

        return await _quic.GetPredictionsAsync(modelBytes, dicts);
    }

    private PredictionDTO ToDTO(PredictionModel model)
    {
        return new PredictionDTO
        {
            Id = model.Id,
            Model = model.Model,
            LastDate = model.LastDate,
            ModelVersion = model.ModelVersion,
            ModelType = model.ModelType
        };
    }
}