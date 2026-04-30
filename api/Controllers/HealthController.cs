using ApiTexPact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HealthController(IHealthService healthService) : ControllerBase
{
    [HttpGet("HeartRate")]
    public async Task<IActionResult> GetHealthData(
        [FromQuery] string smartwatchId,
        [FromQuery] DateTime? startTime = null,
        [FromQuery] DateTime? endTime = null)
    {
        var heartRates = await healthService.GetHeartRate(smartwatchId, startTime, endTime);
        return Ok(heartRates);
    }
    
    [HttpGet("Steps")]
    public async Task<IActionResult> GetSteps(
        [FromQuery] string smartwatchId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var steps = await healthService.GetTotalSteps(smartwatchId, startTime, endTime);
        return Ok(steps);
    }
    
    [HttpGet("ActiveTime")]
    public async Task<IActionResult> GetActiveTime(
        [FromQuery] string smartwatchId,
        [FromQuery] DateTime startTime,
        [FromQuery] DateTime endTime)
    {
        var steps = await healthService.CalculateActiveTime(smartwatchId, startTime, endTime);
        return Ok(steps);
    }
}