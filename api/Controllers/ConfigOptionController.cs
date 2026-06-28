using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigOptionController : ControllerBase
{
    private readonly IConfigOptionRepository _repo;
    private readonly IAuditService           _audit;

    public ConfigOptionController(IConfigOptionRepository repo, IAuditService audit)
    {
        _repo  = repo;
        _audit = audit;
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
        var entity  = new ConfigOptionModel { ConfigId = dto.ConfigId, Value = dto.Value, IsDefault = dto.IsDefault };
        var created = await _repo.Create(entity);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "created", "config_option", created.Id, created.Value);

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ConfigOptionDTO(created.Id, created.ConfigId, created.Value, created.IsDefault));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        await _repo.Delete(id);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "config_option", id, entity.Value);

        return NoContent();
    }
}
