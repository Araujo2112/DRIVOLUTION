using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class ManufacturingPhaseController : ControllerBase
{
    private readonly IManufacturingPhaseRepository _repo;
    public ManufacturingPhaseController(IManufacturingPhaseRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(mp => new ManufacturingPhaseDTO(
            mp.Id, mp.Name, mp.EstimatedDuration,
            mp.MaxAcceptableSeverity, mp.ReworkSeverity)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ManufacturingPhaseDTO(
            item.Id, item.Name, item.EstimatedDuration,
            item.MaxAcceptableSeverity, item.ReworkSeverity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManufacturingPhaseDTO dto)
    {
        var entity = new ManufacturingPhaseModel
        {
            Name = dto.Name,
            EstimatedDuration = dto.EstimatedDuration,
            MaxAcceptableSeverity = dto.MaxAcceptableSeverity ?? Severity.None,
            ReworkSeverity = dto.ReworkSeverity ?? Severity.Minor
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ManufacturingPhaseDTO(created.Id, created.Name, created.EstimatedDuration,
                created.MaxAcceptableSeverity, created.ReworkSeverity));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManufacturingPhaseDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.EstimatedDuration != null) entity.EstimatedDuration = dto.EstimatedDuration;
        if (dto.MaxAcceptableSeverity != null) entity.MaxAcceptableSeverity = dto.MaxAcceptableSeverity;
        if (dto.ReworkSeverity != null) entity.ReworkSeverity = dto.ReworkSeverity;
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