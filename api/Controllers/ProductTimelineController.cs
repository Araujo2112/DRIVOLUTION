using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/products")]
[Authorize(Roles = "admin,manager,operator")]
public class ProductTimelineController : ControllerBase
{
    // Service onde está toda a lógica da timeline dos produtos
    private readonly IProductTimelineService _service;

    // O ASP.NET entrega automaticamente o service ao criar o controller
    public ProductTimelineController(IProductTimelineService service)
    {
        _service = service;
    }

    // Devolve a timeline de um produto através do seu ID
    [HttpGet("{productId}/timeline")]
    public async Task<IActionResult> GetProductTimeline(int productId)
    {
        // Verifica se o produto existe
        if (!await _service.ProductExists(productId))
            return NotFound("Product does not exist.");

        // Pede ao service a timeline do produto
        var result = await _service.GetTimeline(productId);
        // Se ainda não existir timeline devolve erro
        if (result == null)
            return BadRequest("Product has no timeline yet.");

        // Devolve 200 OK com a timeline
        return Ok(result);
    }

    // Devolve a timeline através do número de série (VIN)
    [HttpGet("vin/{serialNumber}/timeline")]
    public async Task<IActionResult> GetProductTimelineBySerial(string serialNumber)
    {
        // Verifica se foi fornecido um número de série
        if (string.IsNullOrWhiteSpace(serialNumber))
            return BadRequest("Serial number is required.");

        // Verifica se existe um produto com esse número de série
        if (!await _service.ProductExistsBySerial(serialNumber))
            return NotFound("Product does not exist.");

        // Pede ao service a timeline do produto
        var result = await _service.GetTimelineBySerial(serialNumber);
        // Se ainda não existir timeline devolve erro
        if (result == null)
            return BadRequest("Product has no timeline yet.");

        // Devolve 200 OK com a timeline
        return Ok(result);
    }
}