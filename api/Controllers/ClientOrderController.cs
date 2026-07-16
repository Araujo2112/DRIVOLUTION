using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/ClientOrder
[Route("api/[controller]")]

// Apenas administradores e gestores podem gerir encomendas
[Authorize(Roles = "admin,manager")]
public class ClientOrderController : ControllerBase
{
    // Service responsável pela lógica de negócio das encomendas
    private readonly IClientOrderService _service;

    // Service responsável por registar ações no log de auditoria
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente os services necessários
    public ClientOrderController(IClientOrderService service, IAuditService audit)
    {
        _service = service;
        _audit = audit;
    }

    // GET /api/ClientOrder
    // Devolve uma lista paginada de encomendas, permitindo aplicar filtros.
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null)
    {
        // Obtém as encomendas através do service
        var result = await _service.GetPaged(page, pageSize, search, status, dateFrom, dateTo);

        return Ok(result);
    }

    // Endpoint legacy para o portal do cliente (client/ClientOrders.vue)
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todas as encomendas (sem paginação)
        var result = await _service.GetPaged(1, int.MaxValue, null, null, null, null);

        // Apenas devolve a lista de encomendas
        return Ok(result.Data);
    }

    // GET /api/ClientOrder/{id}
    // Devolve uma encomenda específica
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetById(id);

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST /api/ClientOrder
    // Cria uma nova encomenda de cliente
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientOrderDTO dto)
    {
        try
        {
            // Cria a encomenda
            var result = await _service.Create(dto);

            // Regista a criação no Audit Log
            var (userId, userName) = User.GetAuditUser();
            await _audit.LogAsync(
                userId,
                userName,
                "created",
                "order",
                result.OrderId,
                $"Encomenda #{result.OrderId}");

            // Devolve HTTP 201 com a encomenda criada
            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            // Ocorre, por exemplo, quando algum recurso necessário não existe
            return NotFound(ex.Message);
        }
    }

    // PUT /api/ClientOrder/{id}
    // Atualiza os dados de uma encomenda existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientOrderDTO dto)
    {
        var updated = await _service.Update(id, dto);

        if (!updated)
            return NotFound();

        // Regista a atualização no Audit Log
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(
            userId,
            userName,
            "updated",
            "order",
            id,
            $"Encomenda #{id}");

        return NoContent();
    }

    // PATCH /api/ClientOrder/{id}/cancel
    // Cancela uma encomenda, caso ainda seja possível
    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var cancelled = await _service.Cancel(id);

        if (!cancelled)
            return BadRequest("Encomenda não encontrada ou não pode ser cancelada (já concluída ou já cancelada).");

        // Regista o cancelamento no Audit Log
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(
            userId,
            userName,
            "updated",
            "order",
            id,
            $"Encomenda #{id} cancelada");

        return NoContent();
    }

    // DELETE /api/ClientOrder/{id}
    // Elimina uma encomenda
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.Delete(id);

        if (!deleted)
            return NotFound();

        // Regista a eliminação no Audit Log
        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(
            userId,
            userName,
            "deleted",
            "order",
            id,
            $"Encomenda #{id}");

        return NoContent();
    }
}