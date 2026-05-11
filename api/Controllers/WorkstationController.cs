using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Workstation;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkstationController : ControllerBase
{
    private readonly IWorkstationRepository _repo;
    public WorkstationController(IWorkstationRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(w => new WorkstationDTO(w.Id, w.ProductionLineId, w.ProductionLine?.Name, w.Type)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new WorkstationDTO(item.Id, item.ProductionLineId, item.ProductionLine?.Name, item.Type));
    }

    [HttpGet("line/{productionLineId}")]
    public async Task<IActionResult> GetByProductionLine(int productionLineId)
    {
        var items = await _repo.GetByProductionLine(productionLineId);
        return Ok(items.Select(w => new WorkstationDTO(w.Id, w.ProductionLineId, null, w.Type)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationDTO dto)
    {
        var entity = new WorkstationModel { ProductionLineId = dto.ProductionLineId, Type = dto.Type };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new WorkstationDTO(created.Id, created.ProductionLineId, null, created.Type));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkstationDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Type != null) entity.Type = dto.Type;
        await _repo.Update(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _repo.Exists(id)) return NotFound();
        await _repo.Delete(id);
        return NoContent();
    }
}