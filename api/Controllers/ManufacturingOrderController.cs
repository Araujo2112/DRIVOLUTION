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
    // Service onde está toda a lógica das ordens de fabrico
    private readonly IManufacturingOrderService _service;

    // O ASP.NET entrega automaticamente o service ao criar o controller
    public ManufacturingOrderController(IManufacturingOrderService service)
    {
        _service = service;
    }

    // Devolve uma lista paginada de ordens de fabrico, podendo aplicar filtros
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        // Pede ao service as ordens de fabrico com os filtros escolhidos
        var result = await _service.GetPaged(page, pageSize, search, status, dateFrom, dateTo);
        // Devolve 200 OK com os resultados
        return Ok(result);
    }

    // Procura uma ordem de fabrico pelo seu ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura a ordem de fabrico
        var item = await _service.GetById(id);
        // Se existir devolve 200, senão devolve 404
        return item == null ? NotFound() : Ok(item);
    }

    // Devolve uma ordem de fabrico com informação mais detalhada
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetWithDetails(int id)
    {
        // Pede ao service a ordem com todos os detalhes
        var item = await _service.GetByIdWithDetails(id);
        // Se existir devolve 200, senão devolve 404
        return item == null ? NotFound() : Ok(item);
    }

    // Cria uma nova ordem de fabrico
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManufacturingOrderDTO dto)
    {
        // Pede ao service para criar a ordem
        var result = await _service.Create(dto);
        // Devolve 201 Created com a ordem criada
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    // Atualiza uma ordem de fabrico existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManufacturingOrderDTO dto)
    {
        // Pede ao service para atualizar a ordem
        var success = await _service.Update(id, dto);
        // Se conseguiu atualizar devolve 204, senão 404
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    // Remove uma ordem de fabrico
    public async Task<IActionResult> Delete(int id)
    {
        // Pede ao service para apagar a ordem
        var success = await _service.Delete(id);
        // Se conseguiu apagar devolve 204, senão 404
        return success ? NoContent() : NotFound();
    }
}