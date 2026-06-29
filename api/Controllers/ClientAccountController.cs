using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/client-accounts")]
[Authorize(Roles = "admin,manager")]
public class ClientAccountController : ControllerBase
{
    private readonly IAuthService  _authService;
    private readonly IUserRepository _userRepo;
    private readonly IAuditService _audit;

    public ClientAccountController(IAuthService authService, IUserRepository userRepo, IAuditService audit)
    {
        _authService = authService;
        _userRepo    = userRepo;
        _audit       = audit;
    }

    // GET /api/client-accounts — lista todos os utilizadores com role=client
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var all = await _userRepo.GetAllAsync();
        var clients = all
            .Where(u => u.Role == "client")
            .Select(u => new ClientOptionDTO(u.Id, u.Name));
        return Ok(clients);
    }

    // POST /api/client-accounts — cria conta de cliente (role forçado, password gerada
    // automaticamente, igual ao fluxo de Manager/Operator em /api/Auth/register)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientAccountDTO dto)
    {
        var (userId, userName) = User.GetAuditUser();

        var request = new RegisterRequestDTO
        {
            Name  = dto.Name,
            Email = dto.Email,
            Role  = "client",
        };

        var result = await _authService.Register(request);
        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        await _audit.LogAsync(userId, userName, "created", "user", result.Value!.User.Id, result.Value.User.Name);

        return Ok(result.Value);
    }

    // PUT /api/client-accounts/{id} — edita nome e email
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientAccountDTO dto)
    {
        var (userId, userName) = User.GetAuditUser();

        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.Role != "client") return NotFound();

        user.Name  = dto.Name;
        user.Email = dto.Email;
        await _userRepo.UpdateAsync(user);

        await _audit.LogAsync(userId, userName, "updated", "user", id, user.Name);

        return Ok();
    }

    // PUT /api/client-accounts/{id}/toggle-status — ativa/desativa (nunca elimina)
    [HttpPut("{id:int}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var (userId, userName) = User.GetAuditUser();

        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.Role != "client") return NotFound();

        user.Status = user.Status == "active" ? "inactive" : "active";
        await _userRepo.UpdateAsync(user);

        var action = user.Status == "active" ? "activated" : "deactivated";
        await _audit.LogAsync(userId, userName, action, "user", id, user.Name);

        return Ok(new { user.Status });
    }

    // PUT /api/client-accounts/{id}/reset-password — gera password temporária nova,
    // mesmo padrão usado para Manager/Operator (não existe "escolher password" no admin).
    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var (userId, userName) = User.GetAuditUser();

        var user = await _userRepo.GetByIdAsync(id);
        if (user == null || user.Role != "client") return NotFound();

        var temporaryPassword     = GenerateTemporaryPassword();
        user.PasswordHash         = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);
        user.MustChangePassword   = true;
        await _userRepo.UpdateAsync(user);

        await _audit.LogAsync(userId, userName, "password_reset", "user", id, user.Name);

        return Ok(new ResetPasswordResponseDTO { TemporaryPassword = temporaryPassword });
    }

    // Mesma lógica de geração usada em UserController — sem caracteres ambíguos.
    private const string PasswordChars           = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
    private const int    TemporaryPasswordLength = 12;

    private static string GenerateTemporaryPassword()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(TemporaryPasswordLength);
        var chars = new char[TemporaryPasswordLength];
        for (int i = 0; i < TemporaryPasswordLength; i++)
            chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];
        return new string(chars);
    }
}