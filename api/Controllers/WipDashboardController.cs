using ApiTexPact.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/production-lines")]
public class WipDashboardController : ControllerBase
{
    private readonly IWipDashboardService _service;

    public WipDashboardController(IWipDashboardService service)
    {
        _service = service;
    }

    [HttpGet("wip")]
    public async Task<IActionResult> GetWipDashboard()
    {
        var result = await _service.GetWipDashboardAsync();
        return Ok(result);
    }
}