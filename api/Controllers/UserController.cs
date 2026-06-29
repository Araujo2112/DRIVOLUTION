using System.Security.Cryptography;
using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
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
    private readonly IAuditService   _audit;

    public UserController(IUserRepository userRepo, IAuditService audit)
    {
        _userRepo = userRepo;
        _audit    = audit;
    }

    // GET /api/User
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userRepo.GetAllAsync();
        return Ok(users.Select(MapToDTO));
    }

    // GET /api/User/clients — lista de contas "client" ativas, para dropdowns
    // (ex: selecionar cliente ao criar uma encomenda). Aberto a admin e manager,
    // que são os roles que podem criar encomendas.
    [HttpGet("clients")]
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GetClients()
    {
        var users = await _userRepo.GetAllAsync();
        var clients = users
            .Where(u => u.Role == "client" && u.Status == "active")
            .Select(u => new ClientOptionDTO(u.Id, u.Name));
        return Ok(clients);
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

        var (auditUserId, auditUserName) = User.GetAuditUser();
        var previousStatus = user.Status;
        var nameOrRoleChanged = false;

        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Trim() != user.Name)
        {
            user.Name = dto.Name.Trim();
            nameOrRoleChanged = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!ValidRoles.Contains(dto.Role))
                return BadRequest($"Role inválido. Valores aceites: {string.Join(", ", ValidRoles)}.");
            if (dto.Role != user.Role) nameOrRoleChanged = true;
            user.Role = dto.Role;
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!ValidStatuses.Contains(dto.Status))
                return BadRequest($"Estado inválido. Valores aceites: {string.Join(", ", ValidStatuses)}.");
            user.Status = dto.Status;
        }

        await _userRepo.UpdateAsync(user);

        // Ativar/desativar é auditado como ação própria, distinta de uma edição normal —
        // é uma ação de segurança (corta/reabre acesso de alguém à plataforma).
        if (previousStatus != user.Status)
        {
            var action = user.Status == "active" ? "activated" : "deactivated";
            await _audit.LogAsync(auditUserId, auditUserName, action, "user", user.Id, user.Name);
        }

        if (nameOrRoleChanged)
        {
            await _audit.LogAsync(auditUserId, auditUserName, "updated", "user", user.Id, user.Name);
        }

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

        var (auditUserId, auditUserName) = User.GetAuditUser();
        await _audit.LogAsync(auditUserId, auditUserName, "password_reset", "user", user.Id, user.Name);

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