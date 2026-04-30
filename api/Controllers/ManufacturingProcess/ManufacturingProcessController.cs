using ApiTexPact.DTO;
using ApiTexPact.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingProcessController : ControllerBase
{
    private readonly IManufacturingProcessService _service;

    public ManufacturingProcessController(IManufacturingProcessService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturingProcessDTO>>> GetAll()
    {
        var processes = await _service.GetAllManufacturingProcesses();
        return Ok(processes);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ManufacturingProcessDTO>> GetById(int id)
    {
        var process = await _service.GetManufacturingProcessById(id);
        if (process == null)
            return NotFound();

        return Ok(process);
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturingProcessDTO>> Create([FromBody] CreateManufacturingProcessDTO dto)
    {
        var created = await _service.CreateManufacturingProcess(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ManufacturingProcessDTO>> Update(int id, [FromBody] UpdateManufacturingProcessDTO dto)
    {
        var updated = await _service.UpdateManufacturingProcess(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteManufacturingProcess(id);
        return NoContent();
    }
}