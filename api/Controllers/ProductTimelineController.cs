using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/products")]
public class ProductTimelineController : ControllerBase
{
    private readonly IProductTimelineRepository _repo;
    private readonly IEtaPredictionService _etaService;

    public ProductTimelineController(IProductTimelineRepository repo, IEtaPredictionService etaService)
    {
        _repo = repo;
        _etaService = etaService;
    }

    [HttpGet("{productId}/timeline")]
    public async Task<IActionResult> GetProductTimeline(int productId)
    {
        if (!await _repo.ProductExists(productId))
            return NotFound("Product does not exist.");

        var timeline = await _repo.GetTimeline(productId);

        if (!timeline.Any())
            return BadRequest("Product has no timeline yet.");

        // Só a fase em curso (EndedAt == null) recebe previsão — é a única
        // que ainda não tem uma duração real conhecida. As fases já
        // concluídas mantêm o DurationSeconds real, sem previsão a mais.
        var openPhase = timeline.FirstOrDefault(t => t.EndedAt == null);
        if (openPhase != null)
        {
            openPhase.EstimatedFinish = await _etaService.PredictCurrentPhaseFinish(productId);
        }

        return Ok(new
        {
            productId,
            modelId      = timeline.First().ModelId,
            serialNumber = timeline.First().SerialNumber,
            status       = timeline.Any(t => t.EndedAt == null) ? "in_progress" : "completed",
            phases       = timeline
        });
    }
}