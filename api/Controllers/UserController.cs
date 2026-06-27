using System.Security.Cryptography;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UserController : ControllerBase
{
    private static readonly string[] ValidRoles   = { "manager", "operator" };
    private static readonly string[] ValidStatuses = { "active", "inactive" };

    // Mesma lógica do AuthService — sem caracteres ambíguos
    private const string PasswordChars          = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
    private const int    TemporaryPasswordLength = 12;

    private readonly IUserRepository _userRepo;

    public UserController(IUserRepository userRepo)
    {
        _userRepo = userRepo;
    }

    // GET /api/User
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepo.GetAllAsync();
        return Ok(users.Select(MapToDTO));
    }

    // PUT /api/User/{id}  — editar nome, role e estado
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequestDTO dto)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null)
            return NotFound("Utilizador não encontrado.");

        // Não pode editar o próprio utilizador admin raiz (role protegido)
        if (user.Role == "admin")
            return BadRequest("Não é possível editar a conta de administrador.");

        if (!string.IsNullOrWhiteSpace(dto.Name))
            user.Name = dto.Name.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!ValidRoles.Contains(dto.Role))
                return BadRequest($"Role inválido. Valores aceites: {string.Join(", ", ValidRoles)}.");
            user.Role = dto.Role;
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!ValidStatuses.Contains(dto.Status))
                return BadRequest($"Estado inválido. Valores aceites: {string.Join(", ", ValidStatuses)}.");
            user.Status = dto.Status;
        }

        await _userRepo.UpdateAsync(user);
        return Ok(MapToDTO(user));
    }

    // POST /api/User/{id}/reset-password  — gera nova password temporária
    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null)
            return NotFound("Utilizador não encontrado.");

        if (user.Role == "admin")
            return BadRequest("Não é possível fazer reset à conta de administrador.");

        var temporaryPassword = GenerateTemporaryPassword();
        user.PasswordHash        = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);
        user.MustChangePassword  = true;

        await _userRepo.UpdateAsync(user);

        return Ok(new ResetPasswordResponseDTO { TemporaryPassword = temporaryPassword });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static UserInfoDTO MapToDTO(Drivolution.Models.UserModel u) => new()
    {
        Id                 = u.Id,
        Name               = u.Name,
        Email              = u.Email,
        Role               = u.Role,
        Status             = u.Status,
        MustChangePassword = u.MustChangePassword,
        CreatedAt          = u.CreatedAt,
    };

    private static string GenerateTemporaryPassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(TemporaryPasswordLength);
        var chars = new char[TemporaryPasswordLength];
        for (var i = 0; i < TemporaryPasswordLength; i++)
            chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];
        return new string(chars);
    }
}