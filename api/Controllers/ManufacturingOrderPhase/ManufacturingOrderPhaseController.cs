using ApiTexPact.DTO;
using ApiTexPact.Services;
using ApiTexPact.Services.Interface.ManufacturingOrderPhase;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingOrderPhaseController : ControllerBase
{
    private readonly IManufacturingOrderPhaseService _service;

    public ManufacturingOrderPhaseController(IManufacturingOrderPhaseService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturingOrderPhaseDTO>>> GetAll()
    {
        var phases = await _service.GetAllPhases();
        return Ok(phases);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ManufacturingOrderPhaseDTO>> GetById(int id)
    {
        var phase = await _service.GetPhaseById(id);
        if (phase == null)
            return NotFound();

        return Ok(phase);
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturingOrderPhaseDTO>> Create([FromBody] CreateManufacturingOrderPhaseDTO dto)
    {
        var created = await _service.CreatePhase(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ManufacturingOrderPhaseDTO>> Update(int id, [FromBody] UpdateManufacturingOrderPhaseDTO dto)
    {
        var updated = await _service.UpdatePhase(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeletePhase(id);
        return NoContent();
    }
}