using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/production-lines")]
public class ProductionLineStatusController : ControllerBase
{
    private readonly IProductionLineStatusService _service;

    public ProductionLineStatusController(IProductionLineStatusService service)
    {
        _service = service;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetProductionLineStatus()
    {
        var status = await _service.GetProductionLineStatusAsync();
        return Ok(status);
    }
}