using ApiTexPact.DTO;
using ApiTexPact.Services.Interface.ManufacturingPhase;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingPhaseController : ControllerBase
{
    private readonly IManufacturingPhaseService _service;

    public ManufacturingPhaseController(IManufacturingPhaseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturingPhaseDTO>>> GetAll()
    {
        var phases = await _service.GetAllManufacturingPhases();
        return Ok(phases);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ManufacturingPhaseDTO>> GetById(int id)
    {
        var phase = await _service.GetManufacturingPhaseById(id);
        if (phase == null)
            return NotFound();

        return Ok(phase);
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturingPhaseDTO>> Create([FromBody] CreateManufacturingPhaseDTO dto)
    {
        var created = await _service.CreateManufacturingPhase(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ManufacturingPhaseDTO>> Update(int id, [FromBody] UpdateManufacturingPhaseDTO dto)
    {
        var updated = await _service.UpdateManufacturingPhase(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteManufacturingPhase(id);
        return NoContent();
    }
}