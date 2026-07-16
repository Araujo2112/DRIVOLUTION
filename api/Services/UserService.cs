using System.Security.Cryptography;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável pela gestão dos utilizadores da equipa
public class UserService : IUserService
{
    // Roles que podem ser atribuídas aos utilizadores da equipa
    private static readonly string[] ValidRoles = { "manager", "operator" };

    // Estados válidos de uma conta
    private static readonly string[] ValidStatuses = { "active", "inactive" };

    // Caracteres utilizados para gerar passwords temporárias
    private const string PasswordChars =
        "ABCDEFGHJKMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";

    // Tamanho da password temporária
    private const int TemporaryPasswordLength = 12;

    // Repository responsável pelos utilizadores
    private readonly IUserRepository _userRepo;

    // Service responsável pelos registos de auditoria
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente as dependências
    public UserService(IUserRepository userRepo, IAuditService audit)
    {
        _userRepo = userRepo;
        _audit = audit;
    }

    // Devolve uma lista paginada dos utilizadores da equipa
    public async Task<PagedResultDTO<UserInfoDTO>> GetTeamPagedAsync(
        int page,
        int pageSize,
        string? search,
        string? role)
    {
        // Obtém os utilizadores através do repository
        var result = await _userRepo.GetTeamPagedAsync(page, pageSize, search, role);

        // Converte os utilizadores para DTO
        return new PagedResultDTO<UserInfoDTO>
        {
            Data = result.Data.Select(MapToDTO),
            Total = result.Total,
            Page = result.Page,
            PageSize = result.PageSize,
        };
    }

    // Devolve apenas os clientes ativos
    public async Task<IEnumerable<ClientOptionDTO>> GetActiveClientsAsync()
    {
        // Obtém todos os utilizadores
        var users = await _userRepo.GetAllAsync();

        // Filtra apenas clientes ativos
        return users
            .Where(u => u.Role == "client" && u.Status == "active")
            .Select(u => new ClientOptionDTO(u.Id, u.Name));
    }

    // Atualiza um utilizador
    public async Task<UserInfoDTO?> UpdateAsync(
        int id,
        UpdateUserRequestDTO dto,
        int auditUserId,
        string auditUserName)
    {
        // Procura o utilizador
        var user = await _userRepo.GetByIdAsync(id);

        // Não permite editar administradores
        if (user is null || user.Role == "admin")
            return null;

        // Guarda o estado anterior para verificar alterações
        var previousStatus = user.Status;

        // Indica se nome ou role foram alterados
        var nameOrRoleChanged = false;

        // Atualiza o nome
        if (!string.IsNullOrWhiteSpace(dto.Name) &&
            dto.Name.Trim() != user.Name)
        {
            user.Name = dto.Name.Trim();
            nameOrRoleChanged = true;
        }

        // Atualiza a role
        if (!string.IsNullOrWhiteSpace(dto.Role))
        {
            // Verifica se a role é válida
            if (!ValidRoles.Contains(dto.Role))
                throw new ArgumentException(
                    $"Role inválido. Valores aceites: {string.Join(", ", ValidRoles)}.");

            if (dto.Role != user.Role)
                nameOrRoleChanged = true;

            user.Role = dto.Role;
        }

        // Atualiza o estado da conta
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            // Verifica se o estado é válido
            if (!ValidStatuses.Contains(dto.Status))
                throw new ArgumentException(
                    $"Estado inválido. Valores aceites: {string.Join(", ", ValidStatuses)}.");

            user.Status = dto.Status;
        }

        // Guarda as alterações
        await _userRepo.UpdateAsync(user);

        // Se o estado mudou, regista na auditoria
        if (previousStatus != user.Status)
        {
            var action = user.Status == "active"
                ? "activated"
                : "deactivated";

            await _audit.LogAsync(
                auditUserId,
                auditUserName,
                action,
                "user",
                user.Id,
                user.Name);
        }

        // Se o nome ou a role mudaram, regista também
        if (nameOrRoleChanged)
        {
            await _audit.LogAsync(
                auditUserId,
                auditUserName,
                "updated",
                "user",
                user.Id,
                user.Name);
        }

        // Devolve o utilizador atualizado
        return MapToDTO(user);
    }

    // Gera uma nova password temporária para um utilizador
    public async Task<string?> ResetPasswordAsync(
        int id,
        int auditUserId,
        string auditUserName)
    {
        // Procura o utilizador
        var user = await _userRepo.GetByIdAsync(id);

        // Não permite alterar passwords de administradores
        if (user is null || user.Role == "admin")
            return null;

        // Gera uma password temporária
        var temporaryPassword = GenerateTemporaryPassword();

        // Guarda a password encriptada
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(temporaryPassword);

        // Obriga o utilizador a alterar a password no próximo login
        user.MustChangePassword = true;

        // Guarda as alterações
        await _userRepo.UpdateAsync(user);

        // Regista a operação na auditoria
        await _audit.LogAsync(
            auditUserId,
            auditUserName,
            "password_reset",
            "user",
            user.Id,
            user.Name);

        // Devolve a password temporária
        return temporaryPassword;
    }

    // ---------- Métodos auxiliares ----------

    // Converte UserModel para UserInfoDTO
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

    // Gera uma password aleatória
    private static string GenerateTemporaryPassword()
    {
        var bytes = RandomNumberGenerator.GetBytes(TemporaryPasswordLength);
        var chars = new char[TemporaryPasswordLength];

        for (var i = 0; i < TemporaryPasswordLength; i++)
            chars[i] = PasswordChars[bytes[i] % PasswordChars.Length];

        return new string(chars);
    }
}