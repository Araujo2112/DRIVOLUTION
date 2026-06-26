using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserModel?> GetByEmailAsync(string email)
        => await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    public async Task<UserModel?> GetByIdAsync(int id)
        => await _context.AppUsers.FindAsync(id);

    public async Task<IEnumerable<UserModel>> GetAllAsync()
        => await _context.AppUsers.OrderBy(u => u.Name).ToListAsync();

    public async Task<UserModel> CreateAsync(UserModel user)
    {
        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
}