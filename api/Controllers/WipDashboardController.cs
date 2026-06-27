using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/production-lines")]
[Authorize(Roles = "admin,manager,operator")]
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