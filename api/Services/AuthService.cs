using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.IdentityModel.Tokens;

namespace Drivolution.Services;

public class AuthService : IAuthService
{
    private static readonly string[] ValidRoles = { "admin", "operator", "client", "manager" };

    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResult<LoginResponseDTO>> Login(LoginRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return AuthResult<LoginResponseDTO>.Fail(AuthErrorCode.InvalidInput, "Email e password são obrigatórios.");

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            return AuthResult<LoginResponseDTO>.Fail(AuthErrorCode.InvalidCredentials, "Credenciais inválidas.");

        if (user.Status != "active")
            return AuthResult<LoginResponseDTO>.Fail(AuthErrorCode.InactiveAccount, "Conta inativa.");

        var token = GenerateToken(user);
        return AuthResult<LoginResponseDTO>.Ok(new LoginResponseDTO { Token = token, User = MapToInfo(user) });
    }

    public async Task<AuthResult<UserInfoDTO>> Register(RegisterRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return AuthResult<UserInfoDTO>.Fail(AuthErrorCode.InvalidInput, "Email e password são obrigatórios.");

        if (!ValidRoles.Contains(dto.Role))
            return AuthResult<UserInfoDTO>.Fail(
                AuthErrorCode.InvalidRole,
                $"Role inválido. Valores aceites: {string.Join(", ", ValidRoles)}.");

        if (await _userRepository.GetByEmailAsync(dto.Email) is not null)
            return AuthResult<UserInfoDTO>.Fail(AuthErrorCode.EmailAlreadyExists, "Já existe um utilizador com este email.");

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
        return AuthResult<UserInfoDTO>.Ok(MapToInfo(created));
    }

    public async Task<UserInfoDTO?> GetUserInfo(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user is null ? null : MapToInfo(user);
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