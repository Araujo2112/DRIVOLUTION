using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ApiTexPact.DTO;
using Microsoft.IdentityModel.Tokens;

namespace ApiTexPact.Services;

public class JwtSettings
{
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
}

public interface IAuthService
{
    Task<string> GenerateToken(EmployeeDto employee);
}

public class AuthService : IAuthService
{
    private readonly JwtSettings _jwtSettings;

    public AuthService(IConfiguration configuration)
    {
        _jwtSettings = new JwtSettings {
            Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? throw new InvalidOperationException(),
            Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? throw new InvalidOperationException(),
            Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? throw new InvalidOperationException()
        };
    }

    public async Task<string> GenerateToken(EmployeeDto employee)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

        var claims = new List<Claim> 
        {
            new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
            new(ClaimTypes.Name, employee.Username),
            new(ClaimTypes.GivenName, employee.FirstName),
            new(ClaimTypes.Surname, employee.LastName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(60),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}