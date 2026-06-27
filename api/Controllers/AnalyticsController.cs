using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("phase-durations")]
    public async Task<IActionResult> GetPhaseDurations()
    {
        var result = await _analyticsService.GetPhaseDurationsAsync();
        return Ok(result);
    }

    [HttpGet("rework-rate")]
    public async Task<IActionResult> GetReworkRate()
    {
        var result = await _analyticsService.GetReworkRatesAsync();
        return Ok(result);
    }

    [HttpGet("throughput")]
    public async Task<IActionResult> GetThroughput()
    {
        var result = await _analyticsService.GetThroughputAsync();
        return Ok(result);
    }
}