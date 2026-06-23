using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportController : ControllerBase
{
    private readonly ISupportRepository _repo;
    public SupportController(ISupportRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(s => new SupportDTO(s.Id, s.ProductionLineId, s.RfidTag, s.Type)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new SupportDTO(item.Id, item.ProductionLineId, item.RfidTag, item.Type));
    }

    [HttpGet("rfid/{tag}")]
    public async Task<IActionResult> GetByRfid(string tag)
    {
        var item = await _repo.GetByRfidTag(tag);
        if (item == null) return NotFound();
        return Ok(new SupportDTO(item.Id, item.ProductionLineId, item.RfidTag, item.Type));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportDTO dto)
    {
        var entity = new SupportModel { ProductionLineId = dto.ProductionLineId, RfidTag = dto.RfidTag, Type = dto.Type };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new SupportDTO(created.Id, created.ProductionLineId, created.RfidTag, created.Type));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupportDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.RfidTag != null) entity.RfidTag = dto.RfidTag;
        if (dto.Type != null) entity.Type = dto.Type;
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