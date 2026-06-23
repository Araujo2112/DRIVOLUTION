using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlertController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _alertService.GetAllAsync());

    [HttpGet("open")]
    public async Task<IActionResult> GetOpen()
        => Ok(await _alertService.GetOpenAsync());

    [HttpPut("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        var result = await _alertService.AcknowledgeAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        var result = await _alertService.ResolveAsync(id);
        if (result == null) return NotFound();
        return Ok(result);
    }
}