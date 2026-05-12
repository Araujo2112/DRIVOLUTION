using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhaseSequenceController : ControllerBase
{
    private readonly IPhaseSequenceRepository _repo;
    public PhaseSequenceController(IPhaseSequenceRepository repo) => _repo = repo;

    [HttpGet("model/{modelId}")]
    public async Task<IActionResult> GetByModel(int modelId)
    {
        var items = await _repo.GetByModel(modelId);
        return Ok(items.Select(ps => new PhaseSequenceDTO(ps.Id, ps.Order, ps.ManufacturingPhaseId, ps.ManufacturingPhase?.Name ?? "", ps.ModelId)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePhaseSequenceDTO dto)
    {
        var entity = new PhaseSequenceModel { Order = dto.Order, ManufacturingPhaseId = dto.ManufacturingPhaseId, ModelId = dto.ModelId };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetByModel), new { modelId = created.ModelId },
            new PhaseSequenceDTO(created.Id, created.Order, created.ManufacturingPhaseId, "", created.ModelId));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePhaseSequenceDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Order != null) entity.Order = dto.Order.Value;
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