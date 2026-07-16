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

// Define a rota base: /api/CarModel
[Route("api/[controller]")]

// Apenas administradores podem gerir modelos de veículos
[Authorize(Roles = "admin")]
public class CarModelController : ControllerBase
{
    // Repository responsável pelos modelos de veículos
    private readonly ICarModelRepository _repo;

    // Service responsável por simular o tempo estimado de fabrico (ETA)
    private readonly ICarModelEtaSimulationService _etaSimulationService;

    // Service responsável por registar ações no log de auditoria
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente os repositories e services necessários
    public CarModelController(ICarModelRepository repo, ICarModelEtaSimulationService etaSimulationService, IAuditService audit)
    {
        _repo = repo;
        _etaSimulationService = etaSimulationService;
        _audit = audit;
    }

    // GET /api/CarModel
    // Devolve todos os modelos de veículos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();

        // Converte os Models em DTOs antes de os devolver ao cliente
        return Ok(items.Select(m => new CarModelDTO(m.Id, m.Name, m.Version, m.Type)));
    }

    // GET /api/CarModel/{id}
    // Devolve um modelo específico
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);

        if (item == null)
            return NotFound();

        return Ok(new CarModelDTO(item.Id, item.Name, item.Version, item.Type));
    }

    // GET /api/CarModel/{id}/phases
    // Devolve o modelo juntamente com a sequência de fases de fabrico
    [HttpGet("{id}/phases")]
    public async Task<IActionResult> GetWithPhases(int id)
    {
        var item = await _repo.GetByIdWithPhaseSequence(id);

        if (item == null)
            return NotFound();

        // Ordena as fases pela sua ordem de execução
        var phases = item.PhaseSequences
            .OrderBy(ps => ps.Order)
            .Select(ps =>
                new PhaseSequenceDTO(
                    ps.Id,
                    ps.Order,
                    ps.ManufacturingPhaseId,
                    ps.ManufacturingPhase?.Name ?? "",
                    ps.ModelId));

        return Ok(new
        {
            model = new CarModelDTO(item.Id, item.Name, item.Version, item.Type),
            phases
        });
    }

    // GET /api/CarModel/{id}/configs
    // Devolve todas as configurações disponíveis para um modelo
    [HttpGet("{id}/configs")]
    public async Task<IActionResult> GetConfigs(int id)
    {
        // Verifica se o modelo existe
        if (!await _repo.Exists(id))
            return NotFound();

        var configs = await _repo.GetConfigs(id);

        return Ok(configs.Select(c =>
            new ConfigDTO(c.Id, c.ModelId, c.Item, c.AllowMultiple)));
    }

    // GET /api/CarModel/{id}/eta-simulation
    // Simula o tempo estimado de fabrico considerando as opções escolhidas
    [HttpGet("{id}/eta-simulation")]
    public async Task<IActionResult> GetEtaSimulation(int id, [FromQuery] List<int>? optionIds)
    {
        var result = await _etaSimulationService.Simulate(id, optionIds ?? new List<int>());

        if (!result.Success)
            return result.ErrorCode switch
            {
                EtaSimulationErrorCode.ModelNotFound => NotFound(result.ErrorMessage),
                EtaSimulationErrorCode.OptionNotFoundForModel => BadRequest(result.ErrorMessage),
                _ => BadRequest(result.ErrorMessage),
            };

        return Ok(result.Value);
    }

    // POST /api/CarModel
    // Cria um novo modelo de veículo
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarModelDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new CarModelModel
        {
            Name = dto.Name,
            Version = dto.Version,
            Type = dto.Type
        };

        var created = await _repo.Create(entity);

        // Regista a criação no log de auditoria
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(
            userId,
            userName,
            "created",
            "car_model",
            created.Id,
            $"{created.Name} {created.Version}");

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            new CarModelDTO(created.Id, created.Name, created.Version, created.Type));
    }

    // PUT /api/CarModel/{id}
    // Atualiza um modelo existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCarModelDTO dto)
    {
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        // Apenas atualiza os campos enviados pelo cliente
        if (dto.Name != null)
            entity.Name = dto.Name;

        if (dto.Version != null)
            entity.Version = dto.Version;

        if (dto.Type != null)
            entity.Type = dto.Type;

        await _repo.Update(entity);

        // Regista a alteração no log de auditoria
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(
            userId,
            userName,
            "updated",
            "car_model",
            entity.Id,
            $"{entity.Name} {entity.Version}");

        return NoContent();
    }

    // DELETE /api/CarModel/{id}
    // Remove um modelo de veículo
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        await _repo.Delete(id);

        // Regista a eliminação no log de auditoria
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(
            userId,
            userName,
            "deleted",
            "car_model",
            id,
            $"{entity.Name} {entity.Version}");

        return NoContent();
    }
}