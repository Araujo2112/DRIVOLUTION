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
public class ProductionLineController : ControllerBase
{
    private readonly IProductionLineRepository _repo;
    private readonly IEtaPredictionService     _etaService;
    private readonly IAuditService             _audit;

    public ProductionLineController(IProductionLineRepository repo, IEtaPredictionService etaService, IAuditService audit)
    {
        _repo       = repo;
        _etaService = etaService;
        _audit      = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(p => new ProductionLineDTO(p.Id, p.Name, p.Location, p.Status, p.Capacity)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ProductionLineDTO(item.Id, item.Name, item.Location, item.Status, item.Capacity));
    }

    [HttpGet("{id}/eta")]
    public async Task<IActionResult> GetEta(int id)
    {
        if (!await _repo.Exists(id)) return NotFound();
        var results = await _etaService.PredictForProductionLine(id);
        return Ok(results);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionLineDTO dto)
    {
        var entity  = new ProductionLineModel { Name = dto.Name, Location = dto.Location, Status = dto.Status, Capacity = dto.Capacity };
        var created = await _repo.Create(entity);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "created", "production_line", created.Id, created.Name);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ProductionLineDTO(created.Id, created.Name, created.Location, created.Status, created.Capacity));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionLineDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Name     != null) entity.Name     = dto.Name;
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.Status   != null) entity.Status   = dto.Status;
        if (dto.Capacity != null) entity.Capacity = dto.Capacity;
        await _repo.Update(entity);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "updated", "production_line", entity.Id, entity.Name);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        await _repo.Delete(id);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "production_line", id, entity.Name);

        return NoContent();
    }
}
