using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Material;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialController : ControllerBase
{
    private readonly IMaterialRepository _repo;
    public MaterialController(IMaterialRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(m => new MaterialDTO(m.Id, m.Item, m.PartNumber)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new MaterialDTO(item.Id, item.Item, item.PartNumber));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialDTO dto)
    {
        var entity = new MaterialModel { Item = dto.Item, PartNumber = dto.PartNumber };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new MaterialDTO(created.Id, created.Item, created.PartNumber));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Item != null) entity.Item = dto.Item;
        if (dto.PartNumber != null) entity.PartNumber = dto.PartNumber;
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