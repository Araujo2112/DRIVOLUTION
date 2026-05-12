using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigController : ControllerBase
{
    private readonly IConfigRepository _repo;
    public ConfigController(IConfigRepository repo) => _repo = repo;

    [HttpGet("model/{modelId}")]
    public async Task<IActionResult> GetByModel(int modelId)
    {
        var items = await _repo.GetByModelId(modelId);
        return Ok(items.Select(c => new ConfigDTO(c.Id, c.ModelId, c.Item)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ConfigDTO(item.Id, item.ModelId, item.Item));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConfigDTO dto)
    {
        var entity = new ConfigModel
        {
            ModelId = dto.ModelId,
            Item = dto.Item
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ConfigDTO(created.Id, created.ModelId, created.Item));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConfigDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        
        if (dto.Item != null) entity.Item = dto.Item;

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