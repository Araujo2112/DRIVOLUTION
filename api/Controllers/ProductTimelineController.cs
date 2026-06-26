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

        return await BuildTimelineResponse(productId, await _repo.GetTimeline(productId));
    }

    [HttpGet("vin/{serialNumber}/timeline")]
    public async Task<IActionResult> GetProductTimelineBySerial(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return BadRequest("Serial number is required.");

        if (!await _repo.ProductExistsBySerial(serialNumber))
            return NotFound("Product does not exist.");

        var timeline = await _repo.GetTimelineBySerial(serialNumber);

        if (!timeline.Any())
            return BadRequest("Product has no timeline yet.");

        return await BuildTimelineResponse(timeline.First().ProductId, timeline);
    }

    private async Task<IActionResult> BuildTimelineResponse(int productId, List<Drivolution.DTO.ProductTimelineDTO> timeline)
    {
        if (!timeline.Any())
            return BadRequest("Product has no timeline yet.");

        var openPhase = timeline.FirstOrDefault(t => t.EndedAt == null);
        if (openPhase != null)
        {
            try
            {
                openPhase.EstimatedFinish = await _etaService.PredictCurrentPhaseFinish(productId);
            }
            catch
            {
                openPhase.EstimatedFinish = null;
            }
        }

        return Ok(new
        {
            productId,
            modelId = timeline.First().ModelId,
            serialNumber = timeline.First().SerialNumber,
            status = timeline.Any(t => t.EndedAt == null) ? "in_progress" : "completed",
            phases = timeline
        });
    }
}