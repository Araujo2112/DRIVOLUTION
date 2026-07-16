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

// Define a rota base: /api/PhaseSequence
[Route("api/[controller]")]

// Apenas administradores podem gerir as sequências de fases
[Authorize(Roles = "admin")]
public class PhaseSequenceController : ControllerBase
{
    // Repository responsável pelas sequências de fases
    private readonly IPhaseSequenceRepository _repo;

    // Repository utilizado para obter informação dos modelos de veículo
    private readonly ICarModelRepository _carModelRepo;

    // Service responsável por registar ações no Audit Log
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente os repositories e services necessários
    public PhaseSequenceController(IPhaseSequenceRepository repo, ICarModelRepository carModelRepo, IAuditService audit)
    {
        _repo = repo;
        _carModelRepo = carModelRepo;
        _audit = audit;
    }

    // GET /api/PhaseSequence/model/{modelId}
    // Devolve a sequência de fases de um determinado modelo de veículo
    [HttpGet("model/{modelId}")]
    public async Task<IActionResult> GetByModel(int modelId)
    {
        // Obtém todas as fases associadas ao modelo
        var items = await _repo.GetByModel(modelId);

        // Converte para DTO e devolve o resultado
        return Ok(items.Select(ps => new PhaseSequenceDTO(
            ps.Id,
            ps.Order,
            ps.ManufacturingPhaseId,
            ps.ManufacturingPhase?.Name ?? "",
            ps.ModelId)));
    }

    // POST /api/PhaseSequence
    // Cria uma nova sequência de fase para um modelo
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePhaseSequenceDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new PhaseSequenceModel
        {
            Order = dto.Order,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
            ModelId = dto.ModelId
        };

        // Guarda a nova sequência
        var created = await _repo.Create(entity);

        // Regista a criação no Audit Log
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "created", "phase_sequence", created.Id, await BuildLabelAsync(created));

        // Devolve 201 Created com a sequência criada
        return CreatedAtAction(nameof(GetByModel), new { modelId = created.ModelId },
            new PhaseSequenceDTO(created.Id, created.Order, created.ManufacturingPhaseId, "", created.ModelId));
    }

    // PUT /api/PhaseSequence/{id}
    // Atualiza a ordem de uma sequência existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePhaseSequenceDTO dto)
    {
        // Procura a sequência
        var entity = await _repo.GetById(id);

        // Se não existir devolve 404
        if (entity == null)
            return NotFound();

        // Atualiza a ordem, caso tenha sido fornecida
        if (dto.Order != null)
            entity.Order = dto.Order.Value;

        // Guarda as alterações
        await _repo.Update(entity);

        // Regista a alteração no Audit Log
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "updated", "phase_sequence", entity.Id, await BuildLabelAsync(entity));

        return NoContent();
    }

    // DELETE /api/PhaseSequence/{id}
    // Remove uma sequência de fases
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura a sequência
        var entity = await _repo.GetById(id);

        // Se não existir devolve 404
        if (entity == null)
            return NotFound();

        // Constrói o texto para o Audit Log antes da remoção
        var label = await BuildLabelAsync(entity);

        // Remove a sequência
        await _repo.Delete(id);

        // Regista a remoção no Audit Log
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "phase_sequence", id, label);

        return NoContent();
    }

    // Inclui o nome do Modelo de Carro (em vez do ID) no label do audit log.
    private async Task<string> BuildLabelAsync(PhaseSequenceModel entity)
    {
        // Obtém o modelo associado à sequência
        var carModel = await _carModelRepo.GetById(entity.ModelId);

        // Utiliza o nome do modelo caso exista, caso contrário utiliza o ID
        var modelName = carModel?.Name ?? $"ID {entity.ModelId}";

        // Constrói a descrição utilizada no Audit Log
        return $"Modelo: {modelName} – Ordem {entity.Order}";
    }
}