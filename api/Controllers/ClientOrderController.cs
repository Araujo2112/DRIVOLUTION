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
    public async Task<IActionResult> GetAll()
    {
        var items = await _service.GetAll();
        return Ok(items);
    }

    [HttpGet("{id}")]
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
