using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkstationStatusController : ControllerBase
{
    private readonly IWorkstationStatusRepository _repo;
    public WorkstationStatusController(IWorkstationStatusRepository repo) => _repo = repo;

    [HttpGet("workstation/{workstationId}")]
    public async Task<IActionResult> GetByWorkstation(int workstationId)
    {
        var items = await _repo.GetByWorkstation(workstationId);
        return Ok(items.Select(ws => new WorkstationStatusDTO(ws.Id, ws.WorkstationId, ws.Status, ws.Timestamp)));
    }

    [HttpGet("workstation/{workstationId}/latest")]
    public async Task<IActionResult> GetLatest(int workstationId)
    {
        var item = await _repo.GetLatestByWorkstation(workstationId);
        if (item == null) return NotFound();
        return Ok(new WorkstationStatusDTO(item.Id, item.WorkstationId, item.Status, item.Timestamp));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationStatusDTO dto)
    {
        var entity = new WorkstationStatusModel 
        { 
            WorkstationId = dto.WorkstationId, 
            Status = dto.Status ?? WorkstationStatusConstants.Functional,
            Timestamp = DateTime.UtcNow 
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetLatest), new { workstationId = created.WorkstationId }, 
            new WorkstationStatusDTO(created.Id, created.WorkstationId, created.Status, created.Timestamp));
    }
}