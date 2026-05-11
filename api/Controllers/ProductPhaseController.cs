using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ProductPhase;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductPhaseController : ControllerBase
{
    private readonly IProductPhaseRepository _repo;
    public ProductPhaseController(IProductPhaseRepository repo) => _repo = repo;

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var items = await _repo.GetByProduct(productId);
        return Ok(items.Select(pp => new ProductPhaseDTO(pp.Id, pp.ProductId, pp.ManufacturingPhaseId, pp.ManufacturingPhase?.Name ?? "", pp.WorkstationId, pp.Notes, pp.Result, pp.Condition, pp.DatetimeIni, pp.DatetimeEnd, pp.QualityId)));
    }

    [HttpGet("product/{productId}/current")]
    public async Task<IActionResult> GetCurrent(int productId)
    {
        var item = await _repo.GetCurrentByProduct(productId);
        if (item == null) return NotFound();
        return Ok(new ProductPhaseDTO(item.Id, item.ProductId, item.ManufacturingPhaseId, item.ManufacturingPhase?.Name ?? "", item.WorkstationId, item.Notes, item.Result, item.Condition, item.DatetimeIni, item.DatetimeEnd, item.QualityId));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductPhaseDTO dto)
    {
        var current = await _repo.GetCurrentByProduct(dto.ProductId);
        if (current != null)
        {
            current.DatetimeEnd = DateTime.UtcNow;
            await _repo.Update(current);
        }
        var entity = new ProductPhaseModel
        {
            ProductId = dto.ProductId,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
            WorkstationId = dto.WorkstationId,
            Notes = dto.Notes,
            DatetimeIni = DateTime.UtcNow
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetCurrent), new { productId = created.ProductId },
            new ProductPhaseDTO(created.Id, created.ProductId, created.ManufacturingPhaseId, "", created.WorkstationId, created.Notes, created.Result, created.Condition, created.DatetimeIni, created.DatetimeEnd, created.QualityId));
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] CloseProductPhaseDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        entity.DatetimeEnd = DateTime.UtcNow;
        entity.Result = dto.Result;
        entity.Condition = dto.Condition;
        entity.QualityId = dto.QualityId;
        await _repo.Update(entity);
        return NoContent();
    }
}