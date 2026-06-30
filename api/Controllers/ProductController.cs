using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,operator")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;
    private readonly IEtaPredictionService _etaService;

    public ProductController(IProductService service, IEtaPredictionService etaService)
    {
        _service = service;
        _etaService = etaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] int? modelId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        var result = await _service.GetPaged(page, pageSize, search, modelId, dateFrom, dateTo);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetById(id);
        return item == null ? NotFound() : Ok(item);
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        var items = await _service.GetByManufacturingOrder(orderId);
        return Ok(items);
    }

    [HttpGet("{id}/eta")]
    public async Task<IActionResult> GetEta(int id)
    {
        var result = await _etaService.PredictForProduct(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
    {
        var result = await _service.Create(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
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