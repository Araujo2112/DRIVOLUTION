using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.LocalizationHistory;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LocalizationHistoryController : ControllerBase
{
    private readonly ILocalizationHistoryRepository _repo;
    public LocalizationHistoryController(ILocalizationHistoryRepository repo) => _repo = repo;

    [HttpGet("support/{supportId}")]
    public async Task<IActionResult> GetBySupport(int supportId)
    {
        var items = await _repo.GetBySupport(supportId);
        return Ok(items.Select(lh => new LocalizationHistoryDTO(lh.Id, lh.SupportId, lh.WorkstationId, lh.Workstation?.Type, lh.DatetimeIni, lh.DatetimeEnd, lh.Status)));
    }

    [HttpGet("support/{supportId}/current")]
    public async Task<IActionResult> GetCurrent(int supportId)
    {
        var item = await _repo.GetCurrentBySupport(supportId);
        if (item == null) return NotFound();
        return Ok(new LocalizationHistoryDTO(item.Id, item.SupportId, item.WorkstationId, item.Workstation?.Type, item.DatetimeIni, item.DatetimeEnd, item.Status));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLocalizationHistoryDTO dto)
    {
        var current = await _repo.GetCurrentBySupport(dto.SupportId);
        if (current != null)
        {
            current.DatetimeEnd = DateTime.UtcNow;
            current.Status = "completed";
            await _repo.Update(current);
        }
        var entity = new LocalizationHistoryModel
        {
            SupportId = dto.SupportId,
            WorkstationId = dto.WorkstationId,
            DatetimeIni = DateTime.UtcNow,
            Status = "active"
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetCurrent), new { supportId = created.SupportId },
            new LocalizationHistoryDTO(created.Id, created.SupportId, created.WorkstationId, null, created.DatetimeIni, created.DatetimeEnd, created.Status));
    }
}