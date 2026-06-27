using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkstationAllocationController : ControllerBase
{
    private readonly IWorkstationAllocationRepository _repo;

    public WorkstationAllocationController(IWorkstationAllocationRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items);
    }

    [HttpGet("workstation/{workstationId}")]
    public async Task<IActionResult> GetByWorkstation(int workstationId)
    {
        var items = await _repo.GetByWorkstation(workstationId);
        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationAllocationDTO dto)
    {
        var entity = new WorkstationAllocationModel
        {
            ResourceId = dto.ResourceId,
            WorkstationId = dto.WorkstationId,
            StartDate = dto.StartDate == default ? DateTime.UtcNow : dto.StartDate,
            Status = dto.Status ?? ActiveStatus.Active 
        };
        
        var created = await _repo.Create(entity);
        
        var resultDTO = new WorkstationAllocationDTO(
            created.Id, 
            created.ResourceId, 
            created.Resource?.IsHuman ?? false, 
            created.WorkstationId, 
            created.Status, 
            created.StartDate, 
            created.EndDate
        );

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, resultDTO);
    }

    [HttpPut("{id}/finish")]
    public async Task<IActionResult> FinishAllocation(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();

        // Quando a alocação termina, mudamos o status e registamos a data de fim
        entity.Status = ActiveStatus.Inactive;
        entity.EndDate = DateTime.UtcNow;

        await _repo.Update(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        
        await _repo.Delete(id);
        return NoContent();
    }
}