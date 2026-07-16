using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/client-accounts
[Route("api/client-accounts")]

// Apenas administradores e gestores podem gerir contas de clientes
[Authorize(Roles = "admin,manager")]
public class ClientAccountController : ControllerBase
{
    // Service responsável pela autenticação e criação de utilizadores
    private readonly IAuthService _authService;

    // Repository responsável pelo acesso aos utilizadores
    private readonly IUserRepository _userRepo;

    // Service responsável por registar ações no log de auditoria
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente os services e repositories necessários
    public ClientAccountController(IAuthService authService, IUserRepository userRepo, IAuditService audit)
    {
        _authService = authService;
        _userRepo = userRepo;
        _audit = audit;
    }

    // GET /api/client-accounts?page=1&pageSize=25&search=
    // Devolve uma lista paginada de clientes, permitindo pesquisar por nome/email.
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? search = null)
    {
        var paged = await _userRepo.GetClientsPagedAsync(page, pageSize, search);

        // Converte os utilizadores para DTO antes de devolver ao frontend
        return Ok(new PagedResultDTO<UserInfoDTO>
        {
            Data = paged.Data.Select(MapToDTO),
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize
        });
    }

    // GET /api/client-accounts/all — lista completa, sem paginação
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todos os utilizadores
        var all = await _userRepo.GetAllAsync();

        // Filtra apenas aqueles cujo role é "client"
        var clients = all
            .Where(u => u.Role == "client")
            .Select(MapToDTO);

        return Ok(clients);
    }

    // POST /api/client-accounts
    // Cria uma nova conta de cliente
    // A password é gerada automaticamente e o role é sempre "client".
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateClientAccountDTO dto)
    {
        // Obtém os dados do utilizador autenticado para auditoria
        var (userId, userName) = User.GetAuditUser();

        // Reutiliza o mecanismo de registo já existente
        var request = new RegisterRequestDTO
        {
            Name = dto.Name,
            Email = dto.Email,
            Role = "client",
        };

        var result = await _authService.Register(request);

        if (!result.Success)
            return BadRequest(result.ErrorMessage);

        // Regista a criação da conta no Audit Log
        await _audit.LogAsync(userId, userName, "created", "user", result.Value!.User.Id, result.Value.User.Name);

        return Ok(result.Value);
    }

    // PUT /api/client-accounts/{id}
    // Atualiza nome, email e estado da conta
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateClientAccountDTO dto)
    {
        var (userId, userName) = User.GetAuditUser();

        // Procura o utilizador
        var user = await _userRepo.GetByIdAsync(id);

        if (user == null || user.Role != "client")
            return NotFound();

        // Guarda o estado anterior para saber se houve alteração
        var previousStatus = user.Status;

        // Verifica se nome ou email foram alterados
        var nameOrEmailChanged = dto.Name != user.Name || dto.Email != user.Email;

        // Atualiza os dados
        user.Name = dto.Name;
        user.Email = dto.Email;

        if (!string.IsNullOrWhiteSpace(dto.Status))
            user.Status = dto.Status;

        await _userRepo.UpdateAsync(user);

        // Mudança de estado é auditada como ação própria (segurança: corta/reabre
        // acesso ao portal), distinta de uma edição normal de nome/email.
        if (previousStatus != user.Status)
        {
            var action = user.Status == "active" ? "activated" : "deactivated";
            await _audit.LogAsync(userId, userName, action, "user", id, user.Name);
        }

        // Se apenas o nome ou email mudou, regista uma atualização normal
        if (nameOrEmailChanged)
            await _audit.LogAsync(userId, userName, "updated", "user", id, user.Name);

        return Ok(MapToDTO(user));
    }

    // PUT /api/client-accounts/{id}/toggle-status
    // Ativa ou desativa uma conta de cliente
    [HttpPut("{id:int}/toggle-status")]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        var (userId, userName) = User.GetAuditUser();

        var user = await _userRepo.GetByIdAsync(id);

        if (user == null || user.Role != "client")
            return NotFound();

        // Alterna entre ativo e inativo
        user.Status = user.Status == "active" ? "inactive" : "active";

        await _userRepo.UpdateAsync(user);

        // Regista a alteração no Audit Log
        var action = user.Status == "active" ? "activated" : "deactivated";
        await _audit.LogAsync(userId, userName, action, "user", id, user.Name);

        return Ok(new { user.Status });
    }

    // PUT /api/client-accounts/{id}/reset-password
    // Gera uma nova password temporária para o cliente.
    [HttpPut("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        var (userId, userName) = User.GetAuditUser();

        var user = await _userRepo.GetByIdAsync(id);

        if (user == null || user.Role != "client")
            return NotFound();

        // Gera uma password temporária aleatória
        var temporaryPassword = GenerateTemporaryPassword();

        // Guarda apenas o hash da password na base de dados
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

        // Obriga o utilizador a alterar a password no próximo login
        user.MustChangePassword = true;

        await _userRepo.UpdateAsync(user);

        // Regista a reposição da password no Audit Log
        await _audit.LogAsync(userId, userName, "password_reset", "user", id, user.Name);

        // Devolve a password temporária ao administrador
        return Ok(new ResetPasswordResponseDTO { TemporaryPassword = temporaryPassword });
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    // Converte um UserModel num DTO apropriado para enviar ao frontend
    private static UserInfoDTO MapToDTO(Drivolution.Models.UserModel u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        Role = u.Role,
        Status = u.Status,
        MustChangePassword = u.MustChangePassword,
        CreatedAt = u.CreatedAt,
    };

    // Conjunto de caracteres permitidos na geração de passwords temporárias.
    // Foram removidos caracteres ambíguos como O/0 ou l/1.
    private const string PasswordChars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";

    // Comprimento da password temporária
    private const int TemporaryPasswordLength = 12;

    // Gera uma password aleatória criptograficamente segura
    private static string GenerateTemporaryPassword()
    {
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(TemporaryPasswordLength);
        var chars = new char[TemporaryPasswordLength];

        for (int i = 0; i < TemporaryPasswordLength; i++)
            chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];

        return new string(chars);
    }
}