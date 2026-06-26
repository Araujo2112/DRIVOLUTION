using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email e password são obrigatórios.");

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return Unauthorized("Credenciais inválidas.");

        if (user.Status != "active")
            return Unauthorized("Conta inativa.");

        var token = GenerateToken(user);
        return Ok(new LoginResponseDTO { Token = token, User = MapToInfo(user) });
    }

    [HttpPost("register")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest("Email e password são obrigatórios.");

        var validRoles = new[] { "admin", "operator", "client", "manager" };
        if (!validRoles.Contains(dto.Role))
            return BadRequest($"Role inválido. Valores aceites: {string.Join(", ", validRoles)}.");

        if (await _userRepository.GetByEmailAsync(dto.Email) is not null)
            return Conflict("Já existe um utilizador com este email.");

        var user = new UserModel
        {
            Name = dto.Name,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = dto.Role,
            Status = "active",
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user);
        return CreatedAtAction(nameof(GetMe), null, MapToInfo(created));
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMe()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
            return Unauthorized();

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null) return NotFound();

        return Ok(MapToInfo(user));
    }

    private string GenerateToken(UserModel user)
    {
        var issuer   = Environment.GetEnvironmentVariable("JWT_ISSUER")   ?? throw new InvalidOperationException("JWT_ISSUER not set");
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException("JWT_AUDIENCE not set");
        var secret   = Environment.GetEnvironmentVariable("JWT_SECRET")   ?? throw new InvalidOperationException("JWT_SECRET not set");

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier,     user.Id.ToString()),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim("name",                        user.Name),
        };

        var token = new JwtSecurityToken(
            issuer:             issuer,
            audience:           audience,
            claims:             claims,
            expires:            DateTime.UtcNow.AddHours(8),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserInfoDTO MapToInfo(UserModel u) => new()
    {
        Id = u.Id, Name = u.Name, Email = u.Email,
        Role = u.Role, Status = u.Status, CreatedAt = u.CreatedAt
    };
}