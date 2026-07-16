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

// Define a rota base: /api/Config
[Route("api/[controller]")]

// Apenas administradores podem gerir configurações dos modelos
[Authorize(Roles = "admin")]
public class ConfigController : ControllerBase
{
    // Repository responsável pelas configurações dos modelos
    private readonly IConfigRepository _repo;

    // Repository utilizado para obter informação dos modelos de veículos
    private readonly ICarModelRepository _carModelRepo;

    // Service responsável pelo registo de auditoria
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente os repositórios e services necessários
    public ConfigController(IConfigRepository repo, ICarModelRepository carModelRepo, IAuditService audit)
    {
        _repo         = repo;
        _carModelRepo = carModelRepo;
        _audit        = audit;
    }

    // GET /api/Config/model/{modelId}
    // Devolve todas as configurações associadas a um modelo de veículo
    [HttpGet("model/{modelId}")]
    public async Task<IActionResult> GetByModel(int modelId)
    {
        var items = await _repo.GetByModelId(modelId);

        return Ok(items.Select(c =>
            new ConfigDTO(c.Id, c.ModelId, c.Item, c.AllowMultiple)));
    }

    // GET /api/Config/{id}
    // Devolve uma configuração específica
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);

        if (item == null)
            return NotFound();

        return Ok(new ConfigDTO(item.Id, item.ModelId, item.Item, item.AllowMultiple));
    }

    // POST /api/Config
    // Cria uma nova configuração para um modelo
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateConfigDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new ConfigModel
        {
            ModelId = dto.ModelId,
            Item = dto.Item,
            AllowMultiple = dto.AllowMultiple
        };

        // Guarda a configuração na base de dados
        var created = await _repo.Create(entity);

        // Regista a operação no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "created",
            "config",
            created.Id,
            await BuildLabelAsync(created));

        // Devolve a configuração criada
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ConfigDTO(created.Id, created.ModelId, created.Item, created.AllowMultiple));
    }

    // PUT /api/Config/{id}
    // Atualiza uma configuração existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateConfigDTO dto)
    {
        // Procura a configuração
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        // Atualiza apenas os campos enviados
        if (dto.Item != null)
            entity.Item = dto.Item;

        if (dto.AllowMultiple != null)
            entity.AllowMultiple = dto.AllowMultiple.Value;

        // Guarda as alterações
        await _repo.Update(entity);

        // Regista a alteração no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "updated",
            "config",
            entity.Id,
            await BuildLabelAsync(entity));

        return NoContent();
    }

    // DELETE /api/Config/{id}
    // Remove uma configuração
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura a configuração
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        // Guarda o texto antes da eliminação para o Audit Log
        var label = await BuildLabelAsync(entity);

        // Remove a configuração
        await _repo.Delete(id);

        // Regista a eliminação no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "deleted",
            "config",
            id,
            label);

        return NoContent();
    }

    // Inclui o nome do Modelo de Carro no label do audit log, para distinguir
    // configs com o mesmo nome em modelos diferentes (ex: "teste" em 2 modelos).
    private async Task<string> BuildLabelAsync(ConfigModel entity)
    {
        // Obtém o modelo associado à configuração
        var carModel = await _carModelRepo.GetById(entity.ModelId);

        // Se existir, devolve o nome da configuração juntamente com o modelo
        return carModel != null
            ? $"{entity.Item} (Modelo: {carModel.Name})"
            : entity.Item;
    }
}