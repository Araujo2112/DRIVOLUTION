using ApiTexPact.DTO;
using ApiTexPact.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingOrderController : ControllerBase
{
    private readonly IManufacturingOrderService _service;

    public ManufacturingOrderController(IManufacturingOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAll());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetWithDetails(int id)
    {
        var item = await _service.GetByIdWithDetails(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status) => Ok(await _service.GetByStatus(status));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManufacturingOrderDTO dto)
    {
        var result = await _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManufacturingOrderDTO dto)
    {
        var success = await _service.Update(id, dto);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _service.Delete(id);
        return success ? NoContent() : NotFound();
    }
}