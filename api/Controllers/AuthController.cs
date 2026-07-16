using System.Security.Claims;
using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/Auth
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Service responsável pela autenticação e gestão de utilizadores
    private readonly IAuthService _authService;

    // O ASP.NET injeta automaticamente o AuthService
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST /api/Auth/login
    // Permite a autenticação de um utilizador
    [HttpPost("login")]
    [AllowAnonymous] // Este endpoint pode ser usado sem estar autenticado
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
    {
        // Tenta autenticar o utilizador
        var result = await _authService.Login(dto);

        // Se o login foi bem sucedido devolve o token JWT e os dados do utilizador
        if (result.Success)
            return Ok(result.Value);

        // Caso contrário devolve o erro adequado
        return result.ErrorCode switch
        {
            AuthErrorCode.InvalidInput => BadRequest(result.ErrorMessage),
            AuthErrorCode.InvalidCredentials => Unauthorized(result.ErrorMessage),
            AuthErrorCode.InactiveAccount => Unauthorized(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage),
        };
    }

    // POST /api/Auth/register
    // Permite criar um novo utilizador
    [HttpPost("register")]
    [Authorize(Roles = "admin,manager")] // Apenas administradores e gestores podem criar utilizadores
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO dto)
    {
        // Cria o novo utilizador
        var result = await _authService.Register(dto);

        // Se foi criado com sucesso devolve HTTP 201
        if (result.Success)
            return CreatedAtAction(nameof(GetMe), null, result.Value);

        // Caso contrário devolve o erro correspondente
        return result.ErrorCode switch
        {
            AuthErrorCode.InvalidInput => BadRequest(result.ErrorMessage),
            AuthErrorCode.InvalidRole => BadRequest(result.ErrorMessage),
            AuthErrorCode.EmailAlreadyExists => Conflict(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage),
        };
    }

    // POST /api/Auth/change-password
    // Permite alterar a palavra-passe do utilizador autenticado
    [HttpPost("change-password")]
    [Authorize(Roles = "admin,manager,operator,client")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO dto)
    {
        // Obtém o ID do utilizador a partir do token JWT
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Se não existir um utilizador autenticado devolve Unauthorized
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Pede ao service para alterar a password
        var result = await _authService.ChangePassword(userId, dto);

        // Se correu tudo bem devolve HTTP 204 (sem conteúdo)
        if (result.Success)
            return NoContent();

        // Caso contrário devolve o erro apropriado
        return result.ErrorCode switch
        {
            AuthErrorCode.InvalidInput => BadRequest(result.ErrorMessage),
            AuthErrorCode.InvalidCurrentPassword => Unauthorized(result.ErrorMessage),
            AuthErrorCode.UserNotFound => NotFound(result.ErrorMessage),
            _ => BadRequest(result.ErrorMessage),
        };
    }

    // GET /api/Auth/me
    // Devolve os dados do utilizador autenticado
    [HttpGet("me")]
    [Authorize(Roles = "admin,manager,operator")]
    public async Task<IActionResult> GetMe()
    {
        // Obtém o ID do utilizador a partir do token JWT
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // Se o token não for válido devolve Unauthorized
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        // Obtém as informações do utilizador
        var user = await _authService.GetUserInfo(userId);

        // Se o utilizador não existir devolve NotFound
        if (user is null)
            return NotFound();

        // Devolve os dados do utilizador
        return Ok(user);
    }
}