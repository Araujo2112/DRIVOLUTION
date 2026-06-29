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
public class ConfigOptionController : ControllerBase
{
    private readonly IConfigOptionRepository _repo;
    private readonly IConfigRepository       _configRepo;
    private readonly ICarModelRepository     _carModelRepo;
    private readonly IAuditService           _audit;

    public ConfigOptionController(
        IConfigOptionRepository repo,
        IConfigRepository configRepo,
        ICarModelRepository carModelRepo,
        IAuditService audit)
    {
        _repo         = repo;
        _configRepo   = configRepo;
        _carModelRepo = carModelRepo;
        _audit        = audit;
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
        await _audit.LogAsync(userId, userName, "created", "config_option", created.Id, await BuildLabelAsync(created));

        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ConfigOptionDTO(created.Id, created.ConfigId, created.Value, created.IsDefault));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();

        var label = await BuildLabelAsync(entity);
        await _repo.Delete(id);

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "config_option", id, label);

        return NoContent();
    }

    // Inclui o item da Config e o nome do Modelo de Carro no label do audit log,
    // para distinguir opções com o mesmo valor em configs/modelos diferentes.
    private async Task<string> BuildLabelAsync(ConfigOptionModel entity)
    {
        var config = await _configRepo.GetById(entity.ConfigId);
        if (config == null) return entity.Value;

        var carModel = await _carModelRepo.GetById(config.ModelId);
        return carModel != null
            ? $"{entity.Value} ({config.Item} — Modelo: {carModel.Name})"
            : $"{entity.Value} ({config.Item})";
    }
}