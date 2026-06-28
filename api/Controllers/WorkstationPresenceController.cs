using System.Security.Claims;
using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkstationPresenceController : ControllerBase
{
    private readonly IWorkstationPresenceService _service;

    public WorkstationPresenceController(IWorkstationPresenceService service)
    {
        _service = service;
    }

    /// <summary>
    /// Check-in do utilizador autenticado numa workstation.
    /// Apenas workstations do tipo human ou hybrid são elegíveis.
    /// </summary>
    [HttpPost("checkin")]
    [Authorize(Roles = "admin,manager,operator")]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDTO dto)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var (success, error, result) = await _service.CheckIn(userId.Value, dto.WorkstationId);

        if (!success) return BadRequest(new { message = error });
        return Ok(result);
    }

    /// <summary>
    /// Check-out do utilizador autenticado numa workstation.
    /// </summary>
    [HttpPut("checkout/{workstationId:int}")]
    [Authorize(Roles = "admin,manager,operator")]
    public async Task<IActionResult> CheckOut(int workstationId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var (success, error) = await _service.CheckOut(userId.Value, workstationId);

        if (!success) return BadRequest(new { message = error });
        return NoContent();
    }

    /// <summary>
    /// Histórico de presenças de uma workstation, com produtos cruzados.
    /// </summary>
    [HttpGet("workstation/{workstationId:int}")]
    [Authorize(Roles = "admin,manager,operator")]
    public async Task<IActionResult> GetByWorkstation(int workstationId)
    {
        var result = await _service.GetByWorkstation(workstationId);
        return Ok(result);
    }

    /// <summary>
    /// Presença ativa do utilizador autenticado numa workstation.
    /// Retorna 404 se não tiver check-in ativo.
    /// </summary>
    [HttpGet("active/{workstationId:int}")]
    [Authorize(Roles = "admin,manager,operator")]
    public async Task<IActionResult> GetActive(int workstationId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _service.GetActive(userId.Value, workstationId);
        if (result == null) return NotFound(new { message = "Sem check-in ativo nesta workstation." });
        return Ok(result);
    }

    /// <summary>
    /// Histórico de presenças do utilizador autenticado.
    /// </summary>
    [HttpGet("mine")]
    [Authorize(Roles = "admin,manager,operator")]
    public async Task<IActionResult> GetMine()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _service.GetByUser(userId.Value);
        return Ok(result);
    }

    // ─── Helper ──────────────────────────────────────────────────────────────

    private int? GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue("sub");
        return int.TryParse(claim, out var id) ? id : null;
    }
}