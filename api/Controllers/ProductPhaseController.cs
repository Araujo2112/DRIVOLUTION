using Drivolution.DTO;
using Microsoft.AspNetCore.Mvc;
using Drivolution.Services.Interface;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductPhaseController : ControllerBase
{
    private readonly IProductPhaseService _service;

    public ProductPhaseController(IProductPhaseService service)
    {
        _service = service;
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var items = await _service.GetByProduct(productId);
        return Ok(items);
    }

    [HttpGet("product/{productId}/current")]
    public async Task<IActionResult> GetCurrent(int productId)
    {
        var item = await _service.GetCurrent(productId);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductPhaseDTO dto)
    {
        var created = await _service.Create(dto);
        return CreatedAtAction(nameof(GetCurrent), new { productId = created.ProductId }, created);
    }

    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] CloseProductPhaseDTO dto)
    {
        try
        {
            await _service.Close(id, dto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}
