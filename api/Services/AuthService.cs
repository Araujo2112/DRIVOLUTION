using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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

    // Sem caracteres ambíguos (0/O, 1/l/I) para facilitar a leitura/transcrição manual.
    private const string PasswordChars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
    private const int TemporaryPasswordLength = 12;

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

    public async Task<AuthResult<RegisterResponseDTO>> Register(RegisterRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Name))
            return AuthResult<RegisterResponseDTO>.Fail(AuthErrorCode.InvalidInput, "Nome e email são obrigatórios.");

        if (!ValidRoles.Contains(dto.Role))
            return AuthResult<RegisterResponseDTO>.Fail(
                AuthErrorCode.InvalidRole,
                $"Role inválido. Valores aceites: {string.Join(", ", ValidRoles)}.");

        if (await _userRepository.GetByEmailAsync(dto.Email) is not null)
            return AuthResult<RegisterResponseDTO>.Fail(AuthErrorCode.EmailAlreadyExists, "Já existe um utilizador com este email.");

        // A password inicial é sempre gerada pelo sistema. O admin que cria a conta
        // nunca a escolhe — apenas a vê uma vez nesta resposta para a entregar à pessoa.
        var temporaryPassword = GenerateTemporaryPassword();

        var user = new UserModel
        {
            Name = dto.Name,
            Email = dto.Email.ToLower().Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword),
            Role = dto.Role,
            Status = "active",
            MustChangePassword = true,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user);

        return AuthResult<RegisterResponseDTO>.Ok(new RegisterResponseDTO
        {
            User = MapToInfo(created),
            TemporaryPassword = temporaryPassword,
        });
    }

    public async Task<AuthResult<bool>> ChangePassword(int userId, ChangePasswordRequestDTO dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return AuthResult<bool>.Fail(AuthErrorCode.InvalidInput, "Password atual e nova password são obrigatórias.");

        if (dto.NewPassword.Length < 8)
            return AuthResult<bool>.Fail(AuthErrorCode.InvalidInput, "A nova password deve ter pelo menos 8 caracteres.");

        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null)
            return AuthResult<bool>.Fail(AuthErrorCode.UserNotFound, "Utilizador não encontrado.");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return AuthResult<bool>.Fail(AuthErrorCode.InvalidCurrentPassword, "Password atual incorreta.");

        if (BCrypt.Net.BCrypt.Verify(dto.NewPassword, user.PasswordHash))
            return AuthResult<bool>.Fail(AuthErrorCode.InvalidInput, "A nova password tem de ser diferente da atual.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.MustChangePassword = false;

        await _userRepository.UpdateAsync(user);
        return AuthResult<bool>.Ok(true);
    }

    public async Task<UserInfoDTO?> GetUserInfo(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user is null ? null : MapToInfo(user);
    }

    private static string GenerateTemporaryPassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(TemporaryPasswordLength);
        var chars = new char[TemporaryPasswordLength];

        for (var i = 0; i < TemporaryPasswordLength; i++)
            chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];

        return new string(chars);
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
        Role = u.Role, Status = u.Status,
        MustChangePassword = u.MustChangePassword,
        CreatedAt = u.CreatedAt
    };
}
