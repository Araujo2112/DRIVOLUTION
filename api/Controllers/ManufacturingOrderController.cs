using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,operator")]
public class ManufacturingOrderController : ControllerBase
{
    private readonly IManufacturingOrderService _service;

    public ManufacturingOrderController(IManufacturingOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var result = await _service.GetPaged(page, pageSize, search, status, dateFrom, dateTo);
        return Ok(result);
    }

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