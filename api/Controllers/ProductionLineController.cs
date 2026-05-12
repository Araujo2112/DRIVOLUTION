using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductionLineController : ControllerBase
{
    private readonly IProductionLineRepository _repo;
    public ProductionLineController(IProductionLineRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        var result = items.Select(p => new ProductionLineDTO(p.Id, p.Name, p.Location, p.Status, p.Capacity));
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ProductionLineDTO(item.Id, item.Name, item.Location, item.Status, item.Capacity));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionLineDTO dto)
    {
        var entity = new ProductionLineModel { Name = dto.Name, Location = dto.Location, Status = dto.Status, Capacity = dto.Capacity };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ProductionLineDTO(created.Id, created.Name, created.Location, created.Status, created.Capacity));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionLineDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.Location != null) entity.Location = dto.Location;
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.Capacity != null) entity.Capacity = dto.Capacity;
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