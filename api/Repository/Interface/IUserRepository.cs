using Drivolution.Models;

namespace Drivolution.Repository.Interface;

public interface IUserRepository
{
    Task<UserModel?> GetByEmailAsync(string email);
    Task<UserModel?> GetByIdAsync(int id);
    Task<IEnumerable<UserModel>> GetAllAsync();
    Task<UserModel> CreateAsync(UserModel user);
}