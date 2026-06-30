using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager")]
public class ClientOrderController : ControllerBase
{
    private readonly IClientOrderService _service;
    private readonly IAuditService       _audit;

    public ClientOrderController(IClientOrderService service, IAuditService audit)
    {
        _service = service;
        _audit   = audit;
    }

    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int      page     = 1,
        [FromQuery] int      pageSize = 25,
        [FromQuery] string?  search   = null,
        [FromQuery] string?  status   = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo   = null)
    {
        var result = await _service.GetPaged(page, pageSize, search, status, dateFrom, dateTo);
        return Ok(result);
    }

    // Endpoint legacy para o portal do cliente (client/ClientOrders.vue)
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetPaged(1, int.MaxValue, null, null, null, null);
        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _service.GetById(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientOrderDTO dto)
    {
        try
        {
            var result = await _service.Create(dto);

            var (userId, userName) = User.GetAuditUser();
            await _audit.LogAsync(userId, userName, "created", "order", result.OrderId, $"Encomenda #{result.OrderId}");

            return CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientOrderDTO dto)
    {
        var updated = await _service.Update(id, dto);
        if (!updated) return NotFound();

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "updated", "order", id, $"Encomenda #{id}");

        return NoContent();
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var cancelled = await _service.Cancel(id);
        if (!cancelled) return BadRequest("Encomenda não encontrada ou não pode ser cancelada (já concluída ou já cancelada).");

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "updated", "order", id, $"Encomenda #{id} cancelada");

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.Delete(id);
        if (!deleted) return NotFound();

        var (userId, userName) = User.GetAuditUser();
        await _audit.LogAsync(userId, userName, "deleted", "order", id, $"Encomenda #{id}");

        return NoContent();
    }
}