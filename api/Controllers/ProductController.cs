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
    // Service responsável pela lógica dos produtos
    private readonly IProductService _service;
    // Service responsável por calcular a ETA prevista
    private readonly IEtaPredictionService _etaService;

    // O ASP.NET entrega automaticamente os dois services
    public ProductController(IProductService service, IEtaPredictionService etaService)
    {
        _service = service;
        _etaService = etaService;
    }

    // Devolve uma lista paginada de produtos, podendo aplicar filtros
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] int? modelId = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        // Pede ao service os produtos com os filtros escolhidos
        var result = await _service.GetPaged(page, pageSize, search, modelId, dateFrom, dateTo);
        // Devolve 200 OK com os resultados
        return Ok(result);
    }

    // Procura um produto pelo seu ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura o produto
        var item = await _service.GetById(id);
        // Se existir devolve 200, senão devolve 404
        return item == null ? NotFound() : Ok(item);
    }

    // Devolve todos os produtos de uma ordem de fabrico
    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        // Procura os produtos dessa ordem
        var items = await _service.GetByManufacturingOrder(orderId);
        // Devolve a lista
        return Ok(items);
    }

    // Devolve a previsão de conclusão (ETA) do produto
    [HttpGet("{id}/eta")]
    public async Task<IActionResult> GetEta(int id)
    {
        // Pede ao serviço de previsão para calcular a ETA
        var result = await _etaService.PredictForProduct(id);
        // Se não encontrar o produto devolve 404
        if (result == null) return NotFound();
        // Caso exista devolve a previsão
        return Ok(result);
    }

    // Cria um novo produto
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
    {
        // Pede ao service para criar o produto
        var result = await _service.Create(dto);
        // Devolve 201 Created com o produto criado
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // Atualiza um produto existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
    {
        // Pede ao service para atualizar o produto
        var success = await _service.Update(id, dto);
        // Se conseguiu atualizar devolve 204, senão 404
        return success ? NoContent() : NotFound();
    }

    // Remove um produto
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Pede ao service para apagar o produto
        var success = await _service.Delete(id);
        // Se conseguiu apagar devolve 204, senão 404
        return success ? NoContent() : NotFound();
    }
}