using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,operator")]
public class LocalizationHistoryController : ControllerBase
{
    private readonly ILocalizationHistoryService _service;

    public LocalizationHistoryController(ILocalizationHistoryService service)
    {
        _service = service;
    }

    [HttpGet("support/{supportId}")]
    public async Task<IActionResult> GetBySupport(int supportId)
    {
        var items = await _service.GetBySupport(supportId);
        return Ok(items);
    }

    [HttpGet("support/{supportId}/current")]
    public async Task<IActionResult> GetCurrent(int supportId)
    {
        var item = await _service.GetCurrent(supportId);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLocalizationHistoryDTO dto)
    {
        var created = await _service.Create(dto);
        return CreatedAtAction(nameof(GetCurrent), new { supportId = created.SupportId }, created);
    }
}
