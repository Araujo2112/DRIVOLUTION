using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

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
        return Ok(items.Select(w => ToDTO(w)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(ToDTO(item));
    }

    [HttpGet("line/{productionLineId}")]
    public async Task<IActionResult> GetByProductionLine(int productionLineId)
    {
        var items = await _repo.GetByProductionLine(productionLineId);
        return Ok(items.Select(w => ToDTO(w)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationDTO dto)
    {
        var entity = new WorkstationModel
        {
            ProductionLineId = dto.ProductionLineId,
            Type = dto.Type,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
        };
        var created = await _repo.Create(entity);
        // Reload para incluir navegação (fase)
        var full = await _repo.GetById(created.Id);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDTO(full!));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkstationDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Type != null) entity.Type = dto.Type;
        if (dto.ManufacturingPhaseId.HasValue) entity.ManufacturingPhaseId = dto.ManufacturingPhaseId;
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

    private static WorkstationDTO ToDTO(WorkstationModel w) => new(
        w.Id,
        w.ProductionLineId,
        w.ProductionLine?.Name,
        w.Type,
        w.ManufacturingPhaseId,
        w.ManufacturingPhase?.Name
    );
}