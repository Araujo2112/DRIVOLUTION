using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IUserService
{
    Task<PagedResultDTO<UserInfoDTO>> GetTeamPagedAsync(int page, int pageSize, string? search, string? role);
    Task<IEnumerable<ClientOptionDTO>> GetActiveClientsAsync();
    Task<UserInfoDTO?> UpdateAsync(int id, UpdateUserRequestDTO dto, int auditUserId, string auditUserName);
    Task<string?> ResetPasswordAsync(int id, int auditUserId, string auditUserName);
}