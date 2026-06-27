using System.Security.Claims;
using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
    {
        var result = await _authService.Login(dto);

        if (result.Success)
            return Ok(result.Value);

        return result.ErrorCode switch
        {
            AuthErrorCode.InvalidInput => BadRequest(result.ErrorMessage),
            AuthErrorCode.InvalidCredentials => Unauthorized(result.ErrorMessage),
            AuthErrorCode.InactiveAccount => Unauthorized(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage),
        };
    }

    [HttpPost("register")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO dto)
    {
        var result = await _authService.Register(dto);

        if (result.Success)
            return CreatedAtAction(nameof(GetMe), null, result.Value);

        return result.ErrorCode switch
        {
            AuthErrorCode.InvalidInput => BadRequest(result.ErrorMessage),
            AuthErrorCode.InvalidRole => BadRequest(result.ErrorMessage),
            AuthErrorCode.EmailAlreadyExists => Conflict(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage),
        };
    }

    [HttpPost("change-password")]
    [Authorize(Roles = "admin,manager,operator,client")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var result = await _authService.ChangePassword(userId, dto);

        if (result.Success)
            return NoContent();

        return result.ErrorCode switch
        {
            AuthErrorCode.InvalidInput => BadRequest(result.ErrorMessage),
            AuthErrorCode.InvalidCurrentPassword => Unauthorized(result.ErrorMessage),
            AuthErrorCode.UserNotFound => NotFound(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage),
        };
    }

    [HttpGet("me")]
    [Authorize(Roles = "admin,manager,operator")]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _authService.GetUserInfo(userId);

        if (user is null)
            return NotFound();

        return Ok(user);
    }
}