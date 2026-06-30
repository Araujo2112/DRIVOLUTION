using System.Security.Cryptography;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class UserService : IUserService
{
    private static readonly string[] ValidRoles    = { "manager", "operator" };
    private static readonly string[] ValidStatuses = { "active", "inactive" };

    private const string PasswordChars          = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
    private const int    TemporaryPasswordLength = 12;

    private readonly IUserRepository _userRepo;
    private readonly IAuditService   _audit;

    public UserService(IUserRepository userRepo, IAuditService audit)
    {
        _userRepo = userRepo;
        _audit    = audit;
    }

    public async Task<PagedResultDTO<UserInfoDTO>> GetTeamPagedAsync(
        int page, int pageSize, string? search, string? role)
    {
        var result = await _userRepo.GetTeamPagedAsync(page, pageSize, search, role);
        return new PagedResultDTO<UserInfoDTO>
        {
            Data     = result.Data.Select(MapToDTO),
            Total    = result.Total,
            Page     = result.Page,
            PageSize = result.PageSize,
        };
    }

    public async Task<IEnumerable<ClientOptionDTO>> GetActiveClientsAsync()
    {
        var users = await _userRepo.GetAllAsync();
        return users
            .Where(u => u.Role == "client" && u.Status == "active")
            .Select(u => new ClientOptionDTO(u.Id, u.Name));
    }

    // Devolve null se o utilizador não existir ou for admin (não editável).
    // Devolve a string de erro se a validação falhar.
    // Devolve o DTO atualizado em caso de sucesso.
    // O controller distingue os casos pelo tipo de retorno — ver UserController.
    public async Task<UserInfoDTO?> UpdateAsync(
        int id, UpdateUserRequestDTO dto, int auditUserId, string auditUserName)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null || user.Role == "admin")
            return null;

        var previousStatus    = user.Status;
        var nameOrRoleChanged = false;

        if (!string.IsNullOrWhiteSpace(dto.Name) && dto.Name.Trim() != user.Name)
        {
            user.Name = dto.Name.Trim();
            nameOrRoleChanged = true;
        }

        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            if (!ValidRoles.Contains(dto.Role))
                throw new ArgumentException($"Role inválido. Valores aceites: {string.Join(", ", ValidRoles)}.");
            if (dto.Role != user.Role) nameOrRoleChanged = true;
            user.Role = dto.Role;
        }

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!ValidStatuses.Contains(dto.Status))
                throw new ArgumentException($"Estado inválido. Valores aceites: {string.Join(", ", ValidStatuses)}.");
            user.Status = dto.Status;
        }

        await _userRepo.UpdateAsync(user);

        if (previousStatus != user.Status)
        {
            var action = user.Status == "active" ? "activated" : "deactivated";
            await _audit.LogAsync(auditUserId, auditUserName, action, "user", user.Id, user.Name);
        }

        if (nameOrRoleChanged)
            await _audit.LogAsync(auditUserId, auditUserName, "updated", "user", user.Id, user.Name);

        return MapToDTO(user);
    }

    // Devolve a password temporária gerada, ou null se o utilizador não existir/for admin.
    public async Task<string?> ResetPasswordAsync(int id, int auditUserId, string auditUserName)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null || user.Role == "admin")
            return null;

        var temporaryPassword   = GenerateTemporaryPassword();
        user.PasswordHash       = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);
        user.MustChangePassword = true;

        await _userRepo.UpdateAsync(user);
        await _audit.LogAsync(auditUserId, auditUserName, "password_reset", "user", user.Id, user.Name);

        return temporaryPassword;
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