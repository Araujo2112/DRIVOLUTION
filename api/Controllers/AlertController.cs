using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,operator")]
public class AlertController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        var result = await _alertService.GetPagedAsync(page, pageSize, type, status);
        return Ok(result);
    }

    [HttpGet("all")]
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