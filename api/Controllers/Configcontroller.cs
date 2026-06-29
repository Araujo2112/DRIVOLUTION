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
public class ConfigController : ControllerBase
{
    private readonly IConfigRepository    _repo;
    private readonly ICarModelRepository  _carModelRepo;
    private readonly IAuditService        _audit;

    public ConfigController(IConfigRepository repo, ICarModelRepository carModelRepo, IAuditService audit)
    {
        _repo         = repo;
        _carModelRepo = carModelRepo;
        _audit        = audit;
    }

    [HttpGet("model/{modelId}")]
    public async Task<IActionResult> GetByModel(int modelId)
    {
        var items = await _repo.GetByModelId(modelId);
        return Ok(items.Select(c => new ConfigDTO(c.Id, c.ModelId, c.Item, c.AllowMultiple)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ConfigDTO(item.Id, item.ModelId, item.Item, item.AllowMultiple));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConfigDTO dto)
    {
        var entity  = new ConfigModel { ModelId = dto.ModelId, Item = dto.Item, AllowMultiple = dto.AllowMultiple };
        var created = await _repo.Create(entity);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "created", "config", created.Id, await BuildLabelAsync(created));

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ConfigDTO(created.Id, created.ModelId, created.Item, created.AllowMultiple));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConfigDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Item          != null) entity.Item         = dto.Item;
        if (dto.AllowMultiple != null) entity.AllowMultiple = dto.AllowMultiple.Value;
        await _repo.Update(entity);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "updated", "config", entity.Id, await BuildLabelAsync(entity));

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();

        var label = await BuildLabelAsync(entity);
        await _repo.Delete(id);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "config", id, label);

        return NoContent();
    }

    // Inclui o nome do Modelo de Carro no label do audit log, para distinguir
    // configs com o mesmo nome em modelos diferentes (ex: "teste" em 2 modelos).
    private async Task<string> BuildLabelAsync(ConfigModel entity)
    {
        var carModel = await _carModelRepo.GetById(entity.ModelId);
        return carModel != null
            ? $"{entity.Item} (Modelo: {carModel.Name})"
            : entity.Item;
    }
}