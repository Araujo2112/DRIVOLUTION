using ApiTexPact.DTO;
using ApiTexPact.Services;
using ApiTexPact.Services.Interface.ManufacturingProcessPhase;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers.ManufacturingProcessPhase;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingProcessPhaseController : ControllerBase
{
    private readonly IManufacturingProcessPhaseService _service;

    public ManufacturingProcessPhaseController(IManufacturingProcessPhaseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturingProcessPhaseDTO>>> GetAll()
    {
        var phases = await _service.GetAllManufacturingProcessPhases();
        return Ok(phases);
    }

    [HttpGet("{manufacturingProcessId}/{manufacturingPhaseId}")]
    public async Task<ActionResult<ManufacturingProcessPhaseDTO>> GetById(int manufacturingProcessId, int manufacturingPhaseId)
    {
        var phase = await _service.GetManufacturingProcessPhaseById(manufacturingProcessId, manufacturingPhaseId);
        if (phase == null)
            return NotFound();

        return Ok(phase);
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturingProcessPhaseDTO>> Create([FromBody] CreateManufacturingProcessPhaseDTO dto)
    {
        var created = await _service.CreateManufacturingProcessPhase(dto);
        return Ok(created);
    }

    [HttpPut("{manufacturingProcessId}/{manufacturingPhaseId}")]
    public async Task<ActionResult<ManufacturingProcessPhaseDTO>> Update(int manufacturingProcessId, int manufacturingPhaseId, [FromBody] UpdateManufacturingProcessPhaseDTO dto)
    {
        var updated = await _service.UpdateManufacturingProcessPhase(manufacturingProcessId, manufacturingPhaseId, dto);
        return Ok(updated);
    }

    [HttpDelete("{manufacturingProcessId}/{manufacturingPhaseId}")]
    public async Task<IActionResult> Delete(int manufacturingProcessId, int manufacturingPhaseId)
    {
        await _service.DeleteManufacturingProcessPhase(manufacturingProcessId, manufacturingPhaseId);
        return NoContent();
    }
}
