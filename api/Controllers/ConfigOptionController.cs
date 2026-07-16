using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/ConfigOption
[Route("api/[controller]")]

// Apenas administradores podem gerir opções de configuração
[Authorize(Roles = "admin")]
public class ConfigOptionController : ControllerBase
{
    // Repository responsável pelas opções de configuração
    private readonly IConfigOptionRepository _repo;

    // Repository utilizado para obter informações da configuração
    private readonly IConfigRepository _configRepo;

    // Repository utilizado para obter informações do modelo de veículo
    private readonly ICarModelRepository _carModelRepo;

    // Service responsável pelo registo de auditoria
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente os repositórios e services necessários
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

    // GET /api/ConfigOption
    // Devolve todas as opções de configuração existentes
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _repo.GetAll());

    // GET /api/ConfigOption/{id}
    // Devolve uma opção de configuração específica
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);

        return item == null
            ? NotFound()
            : Ok(item);
    }

    // POST /api/ConfigOption
    // Cria uma nova opção para uma configuração
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConfigOptionDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new ConfigOptionModel
        {
            ConfigId = dto.ConfigId,
            Value = dto.Value,
            IsDefault = dto.IsDefault
        };

        // Guarda a nova opção na base de dados
        var created = await _repo.Create(entity);

        // Regista a criação no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "created",
            "config_option",
            created.Id,
            await BuildLabelAsync(created));

        // Devolve a opção criada
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            new ConfigOptionDTO(created.Id, created.ConfigId, created.Value, created.IsDefault));
    }

    // DELETE /api/ConfigOption/{id}
    // Remove uma opção de configuração
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura a opção
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        // Guarda o texto para o Audit Log antes da eliminação
        var label = await BuildLabelAsync(entity);

        // Remove a opção
        await _repo.Delete(id);

        // Regista a eliminação no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "deleted",
            "config_option",
            id,
            label);

        return NoContent();
    }

    // Inclui o item da Config e o nome do Modelo de Carro no label do audit log,
    // para distinguir opções com o mesmo valor em configs/modelos diferentes.
    private async Task<string> BuildLabelAsync(ConfigOptionModel entity)
    {
        // Obtém a configuração à qual esta opção pertence
        var config = await _configRepo.GetById(entity.ConfigId);

        if (config == null)
            return entity.Value;

        // Obtém o modelo associado à configuração
        var carModel = await _carModelRepo.GetById(config.ModelId);

        // Constrói uma descrição mais informativa para o Audit Log
        return carModel != null
            ? $"{entity.Value} ({config.Item} — Modelo: {carModel.Name})"
            : $"{entity.Value} ({config.Item})";
    }
}