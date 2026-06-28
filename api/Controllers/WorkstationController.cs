using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkstationController : ControllerBase
{
    private readonly IWorkstationRepository _repo;
    private readonly IAuditService          _audit;

    public WorkstationController(IWorkstationRepository repo, IAuditService audit)
    {
        _repo  = repo;
        _audit = audit;
    }

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

    /// <summary>Workstations elegíveis para presença de operadores (human e hybrid).</summary>
    [HttpGet("human-eligible")]
    public async Task<IActionResult> GetHumanEligible()
    {
        var items    = await _repo.GetAll();
        var eligible = items.Where(w => WorkstationKind.HumanEligible.Contains(w.Kind));
        return Ok(eligible.Select(w => ToDTO(w)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationDTO dto)
    {
        var entity = new WorkstationModel
        {
            ProductionLineId     = dto.ProductionLineId,
            Type                 = dto.Type,
            Kind                 = dto.Kind,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
        };
        var created = await _repo.Create(entity);
        var full    = await _repo.GetById(created.Id);

        var (userId, userName) = User.GetAuditUser();
        var label = $"{full!.Type} (Linha {full.ProductionLine?.Name ?? full.ProductionLineId.ToString()})";
        await _audit.LogAsync(userId, userName, "created", "workstation", created.Id, label);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, ToDTO(full!));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateWorkstationDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Type                 != null)  entity.Type                 = dto.Type;
        if (dto.Kind                 != null)  entity.Kind                 = dto.Kind;
        if (dto.ManufacturingPhaseId.HasValue) entity.ManufacturingPhaseId = dto.ManufacturingPhaseId;
        await _repo.Update(entity);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "updated", "workstation", entity.Id, entity.Type ?? $"Workstation {entity.Id}");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        await _repo.Delete(id);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "workstation", id, entity.Type ?? $"Workstation {id}");

        return NoContent();
    }

    private static WorkstationDTO ToDTO(WorkstationModel w) => new(
        w.Id,
        w.ProductionLineId,
        w.ProductionLine?.Name,
        w.Type,
        w.Kind,
        w.ManufacturingPhaseId,
        w.ManufacturingPhase?.Name
    );
}
