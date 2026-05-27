using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/products")]
public class ProductTimelineController : ControllerBase
{
    private readonly IProductTimelineRepository _repo;

    public ProductTimelineController(IProductTimelineRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("{productId}/timeline")]
    public async Task<IActionResult> GetProductTimeline(int productId)
    {
        if (!await _repo.ProductExists(productId))
            return NotFound("Product does not exist.");

        var timeline = await _repo.GetTimeline(productId);

        if (!timeline.Any())
            return BadRequest("Product has no timeline yet.");

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