using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.QualityCheck;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class QualityCheckController : ControllerBase
{
    private readonly IQualityCheckRepository _repo;
    public QualityCheckController(IQualityCheckRepository repo) => _repo = repo;

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var items = await _repo.GetByProduct(productId);
        return Ok(items.Select(qc => new QualityCheckDTO(qc.Id, qc.ProductId, qc.ManufacturingPhaseId, qc.Notes, qc.Status)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new QualityCheckDTO(item.Id, item.ProductId, item.ManufacturingPhaseId, item.Notes, item.Status));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQualityCheckDTO dto)
    {
        var entity = new QualityCheckModel { ProductId = dto.ProductId, ManufacturingPhaseId = dto.ManufacturingPhaseId, Notes = dto.Notes, Status = dto.Status };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new QualityCheckDTO(created.Id, created.ProductId, created.ManufacturingPhaseId, created.Notes, created.Status));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateQualityCheckDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Notes != null) entity.Notes = dto.Notes;
        if (dto.Status != null) entity.Status = dto.Status;
        await _repo.Update(entity);
        return NoContent();
    }
}