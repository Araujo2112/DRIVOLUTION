using Drivolution.DTO;
using Drivolution.Models;

namespace Drivolution.Repository.Interface;

public interface IUserRepository
{
    Task<PagedResultDTO<UserModel>> GetClientsPagedAsync(int page, int pageSize, string? search);
    Task<UserModel?> GetByEmailAsync(string email);
    Task<UserModel?> GetByIdAsync(int id);
    Task<IEnumerable<UserModel>> GetAllAsync();
    Task<UserModel> CreateAsync(UserModel user);
    Task<UserModel> UpdateAsync(UserModel user);
}