using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/ManufacturingPhase
[Route("api/[controller]")]

// Apenas administradores podem gerir as fases de fabrico
[Authorize(Roles = "admin")]
public class ManufacturingPhaseController : ControllerBase
{
    // Repository responsável pelas fases de fabrico
    private readonly IManufacturingPhaseRepository _repo;

    // Service responsável pelo registo de auditoria
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente os repositórios e services necessários
    public ManufacturingPhaseController(IManufacturingPhaseRepository repo, IAuditService audit)
    {
        _repo  = repo;
        _audit = audit;
    }

    // GET /api/ManufacturingPhase
    // Devolve todas as fases de fabrico existentes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todas as fases
        var items = await _repo.GetAll();

        // Converte as entidades em DTOs antes de devolver ao cliente
        return Ok(items.Select(mp => new ManufacturingPhaseDTO(
            mp.Id,
            mp.Name,
            mp.EstimatedDuration,
            mp.MaxAcceptableSeverity,
            mp.ReworkSeverity)));
    }

    // GET /api/ManufacturingPhase/{id}
    // Devolve uma fase de fabrico específica
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura a fase pelo ID
        var item = await _repo.GetById(id);

        if (item == null)
            return NotFound();

        // Devolve a fase encontrada
        return Ok(new ManufacturingPhaseDTO(
            item.Id,
            item.Name,
            item.EstimatedDuration,
            item.MaxAcceptableSeverity,
            item.ReworkSeverity));
    }

    // POST /api/ManufacturingPhase
    // Cria uma nova fase de fabrico
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManufacturingPhaseDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new ManufacturingPhaseModel
        {
            Name                  = dto.Name,
            EstimatedDuration     = dto.EstimatedDuration,

            // Caso não sejam enviados valores, utiliza os valores por defeito
            MaxAcceptableSeverity = dto.MaxAcceptableSeverity ?? Severity.None,
            ReworkSeverity        = dto.ReworkSeverity ?? Severity.Minor,
        };

        // Guarda a nova fase na base de dados
        var created = await _repo.Create(entity);

        // Regista a criação no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "created",
            "phase",
            created.Id,
            created.Name);

        // Devolve a fase criada
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            new ManufacturingPhaseDTO(
                created.Id,
                created.Name,
                created.EstimatedDuration,
                created.MaxAcceptableSeverity,
                created.ReworkSeverity));
    }

    // PUT /api/ManufacturingPhase/{id}
    // Atualiza uma fase de fabrico existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManufacturingPhaseDTO dto)
    {
        // Procura a fase
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        // Atualiza apenas os campos enviados
        if (dto.Name != null)
            entity.Name = dto.Name;

        if (dto.EstimatedDuration != null)
            entity.EstimatedDuration = dto.EstimatedDuration;

        if (dto.MaxAcceptableSeverity != null)
            entity.MaxAcceptableSeverity = dto.MaxAcceptableSeverity;

        if (dto.ReworkSeverity != null)
            entity.ReworkSeverity = dto.ReworkSeverity;

        // Guarda as alterações
        await _repo.Update(entity);

        // Regista a alteração no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "updated",
            "phase",
            entity.Id,
            entity.Name);

        return NoContent();
    }

    // DELETE /api/ManufacturingPhase/{id}
    // Remove uma fase de fabrico
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura a fase
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        // Remove a fase
        await _repo.Delete(id);

        // Regista a eliminação no Audit Log
        var (userId, userName) = User.GetAuditUser();

        await _audit.LogAsync(
            userId,
            userName,
            "deleted",
            "phase",
            id,
            entity.Name);

        return NoContent();
    }
}