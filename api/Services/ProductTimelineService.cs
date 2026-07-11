using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ProductTimelineService : IProductTimelineService
{
    private readonly IProductTimelineRepository _repository;
    private readonly IEtaPredictionService _etaService;

    public ProductTimelineService(
        IProductTimelineRepository repository,
        IEtaPredictionService etaService)
    {
        _repository = repository;
        _etaService = etaService;
    }

    public Task<bool> ProductExists(int productId) => _repository.ProductExists(productId);

    public Task<bool> ProductExistsBySerial(string serialNumber) =>
        _repository.ProductExistsBySerial(serialNumber);

    public async Task<ProductTimelineResultDTO?> GetTimeline(int productId)
    {
        var timeline = await _repository.GetTimeline(productId);
        return await BuildResult(timeline);
    }

    public async Task<ProductTimelineResultDTO?> GetTimelineBySerial(string serialNumber)
    {
        var timeline = await _repository.GetTimelineBySerial(serialNumber);
        return await BuildResult(timeline);
    }

    private async Task<ProductTimelineResultDTO?> BuildResult(List<ProductTimelineDTO> timeline)
    {
        if (!timeline.Any())
            return null;

        var openPhase = timeline.FirstOrDefault(t => t.EndedAt == null);

        if (openPhase != null)
        {
            try
            {
                var prediction = await _etaService.PredictCurrentPhaseDurationSeconds(openPhase.ProductId);

                if (prediction != null)
                {
                    var estimatedFinish = openPhase.StartedAt.AddSeconds(prediction.Seconds);

                    if (estimatedFinish < DateTime.UtcNow)
                        estimatedFinish = DateTime.UtcNow;

                    openPhase.EstimatedFinish = estimatedFinish;
                }
                else
                {
                    openPhase.EstimatedFinish = null;
                }
            }
            catch
            {
                openPhase.EstimatedFinish = null;
            }
        }

        return new ProductTimelineResultDTO
        {
            ProductId = timeline.First().ProductId,
            ModelId = timeline.First().ModelId,
            SerialNumber = timeline.First().SerialNumber,
            Status = timeline.Any(t => t.EndedAt == null) ? "in_progress" : "completed",
            Phases = timeline
        };
    }
}