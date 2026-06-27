using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Roles = "admin,manager,operator")]
public class ProductTimelineController : ControllerBase
{
    private readonly IProductTimelineService _service;

    public ProductTimelineController(IProductTimelineService service)
    {
        _service = service;
    }

    [HttpGet("{productId}/timeline")]
    public async Task<IActionResult> GetProductTimeline(int productId)
    {
        if (!await _service.ProductExists(productId))
            return NotFound("Product does not exist.");

        var result = await _service.GetTimeline(productId);
        if (result == null)
            return BadRequest("Product has no timeline yet.");

        return Ok(result);
    }

    [HttpGet("vin/{serialNumber}/timeline")]
    public async Task<IActionResult> GetProductTimelineBySerial(string serialNumber)
    {
        if (string.IsNullOrWhiteSpace(serialNumber))
            return BadRequest("Serial number is required.");

        if (!await _service.ProductExistsBySerial(serialNumber))
            return NotFound("Product does not exist.");

        var result = await _service.GetTimelineBySerial(serialNumber);
        if (result == null)
            return BadRequest("Product has no timeline yet.");

        return Ok(result);
    }
}