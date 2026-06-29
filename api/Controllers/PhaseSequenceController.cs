using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class PhaseSequenceController : ControllerBase
{
    private readonly IPhaseSequenceRepository _repo;
    private readonly ICarModelRepository      _carModelRepo;
    private readonly IAuditService            _audit;

    public PhaseSequenceController(IPhaseSequenceRepository repo, ICarModelRepository carModelRepo, IAuditService audit)
    {
        _repo         = repo;
        _carModelRepo = carModelRepo;
        _audit        = audit;
    }

    [HttpGet("model/{modelId}")]
    public async Task<IActionResult> GetByModel(int modelId)
    {
        var items = await _repo.GetByModel(modelId);
        return Ok(items.Select(ps => new PhaseSequenceDTO(ps.Id, ps.Order, ps.ManufacturingPhaseId, ps.ManufacturingPhase?.Name ?? "", ps.ModelId)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePhaseSequenceDTO dto)
    {
        var entity  = new PhaseSequenceModel { Order = dto.Order, ManufacturingPhaseId = dto.ManufacturingPhaseId, ModelId = dto.ModelId };
        var created = await _repo.Create(entity);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "created", "phase_sequence", created.Id, await BuildLabelAsync(created));

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

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "updated", "phase_sequence", entity.Id, await BuildLabelAsync(entity));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();

        var label = await BuildLabelAsync(entity);
        await _repo.Delete(id);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "phase_sequence", id, label);

        return NoContent();
    }

    // Inclui o nome do Modelo de Carro (em vez do ID) no label do audit log.
    private async Task<string> BuildLabelAsync(PhaseSequenceModel entity)
    {
        var carModel = await _carModelRepo.GetById(entity.ModelId);
        var modelName = carModel?.Name ?? $"ID {entity.ModelId}";
        return $"Modelo: {modelName} – Ordem {entity.Order}";
    }
}