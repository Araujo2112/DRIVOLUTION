using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigOptionController : ControllerBase
{
    private readonly IConfigOptionRepository _repo;

    public ConfigOptionController(IConfigOptionRepository repo)
    {
        _repo = repo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _repo.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

[HttpPost]
public async Task<IActionResult> Create([FromBody] CreateConfigOptionDTO dto)
{
    var entity = new ConfigOptionModel
    {
        ConfigId = dto.ConfigId,
        Value = dto.Value,
        IsDefault = dto.IsDefault
    };
    
    var created = await _repo.Create(entity);
    
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, 
        new ConfigOptionDTO(created.Id, created.ConfigId, created.Value, created.IsDefault));
}

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _repo.Exists(id)) return NotFound();
        await _repo.Delete(id);
        return NoContent();
    }
}