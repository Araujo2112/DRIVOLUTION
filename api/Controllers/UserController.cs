using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /api/User
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int     page     = 1,
        [FromQuery] int     pageSize = 25,
        [FromQuery] string? search   = null,
        [FromQuery] string? role     = null)
    {
        var result = await _userService.GetTeamPagedAsync(page, pageSize, search, role);
        return Ok(result);
    }

    // GET /api/User/clients — lista de contas "client" ativas, para dropdowns
    [HttpGet("clients")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GetClients()
    {
        var clients = await _userService.GetActiveClientsAsync();
        return Ok(clients);
    }

    // PUT /api/User/{id}
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequestDTO dto)
    {
        var (auditUserId, auditUserName) = User.GetAuditUser();
        try
        {
            var result = await _userService.UpdateAsync(id, dto, auditUserId, auditUserName);
            if (result is null)
                return NotFound("Utilizador não encontrado ou não editável.");
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // POST /api/User/{id}/reset-password
    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var (auditUserId, auditUserName) = User.GetAuditUser();
        var temporaryPassword = await _userService.ResetPasswordAsync(id, auditUserId, auditUserName);
        if (temporaryPassword is null)
            return NotFound("Utilizador não encontrado ou não editável.");
        return Ok(new ResetPasswordResponseDTO { TemporaryPassword = temporaryPassword });
    }
}