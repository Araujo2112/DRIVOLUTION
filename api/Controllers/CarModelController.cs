using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CarModelController : ControllerBase
{
    private readonly ICarModelRepository _repo;
    private readonly ICarModelEtaSimulationService _etaSimulationService;

    public CarModelController(ICarModelRepository repo, ICarModelEtaSimulationService etaSimulationService)
    {
        _repo = repo;
        _etaSimulationService = etaSimulationService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(m => new CarModelDTO(m.Id, m.Name, m.Version, m.Type)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new CarModelDTO(item.Id, item.Name, item.Version, item.Type));
    }

    [HttpGet("{id}/phases")]
    public async Task<IActionResult> GetWithPhases(int id)
    {
        var item = await _repo.GetByIdWithPhaseSequence(id);
        if (item == null) return NotFound();
        var phases = item.PhaseSequences.OrderBy(ps => ps.Order).Select(ps =>
            new PhaseSequenceDTO(ps.Id, ps.Order, ps.ManufacturingPhaseId, ps.ManufacturingPhase?.Name ?? "", ps.ModelId));
        return Ok(new { model = new CarModelDTO(item.Id, item.Name, item.Version, item.Type), phases });
    }

    [HttpGet("{id}/configs")]
    public async Task<IActionResult> GetConfigs(int id)
    {
        if (!await _repo.Exists(id)) return NotFound();
        var configs = await _repo.GetConfigs(id);
        return Ok(configs.Select(c => new ConfigDTO(c.Id, c.ModelId, c.Item, c.AllowMultiple)));
    }

    // Simula o tempo de fabrico de um modelo com uma combinação de opções de
    // configuração, sem criar nenhum produto ou encomenda real.
    // optionIds aceita tanto ?optionIds=3&optionIds=7 como ?optionIds=3,7 (o
    // model binder do ASP.NET Core trata ambos para List<int>). Não cria
    // nenhum Product/ManufacturingOrder — só lê coeficientes já treinados.
    [HttpGet("{id}/eta-simulation")]
    public async Task<IActionResult> GetEtaSimulation(int id, [FromQuery] List<int>? optionIds)
    {
        var result = await _etaSimulationService.Simulate(id, optionIds ?? new List<int>());

        if (!result.Success)
        {
            return result.ErrorCode switch
            {
                EtaSimulationErrorCode.ModelNotFound => NotFound(result.ErrorMessage),
                EtaSimulationErrorCode.OptionNotFoundForModel => BadRequest(result.ErrorMessage),
                _ => BadRequest(result.ErrorMessage),
            };
        }

        return Ok(result.Value);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCarModelDTO dto)
    {
        var entity = new CarModelModel { Name = dto.Name, Version = dto.Version, Type = dto.Type };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new CarModelDTO(created.Id, created.Name, created.Version, created.Type));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCarModelDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Name != null) entity.Name = dto.Name;
        if (dto.Version != null) entity.Version = dto.Version;
        if (dto.Type != null) entity.Type = dto.Type;
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