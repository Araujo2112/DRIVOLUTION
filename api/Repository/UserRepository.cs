using Drivolution.Data;
using Drivolution.DTO;
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

    // Paginação restrita a role="client" — só usada pela página de gestão de
    // Clientes. A Equipa (admin/manager/operator) usa GetTeamPagedAsync, abaixo.
    public async Task<PagedResultDTO<UserModel>> GetClientsPagedAsync(int page, int pageSize, string? search)
    {
        var query = _context.AppUsers.Where(u => u.Role == "client");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(s));
        }

        var total = await query.CountAsync();

        var data = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDTO<UserModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Paginação da Equipa (admin/manager/operator) — exclui role="client",
    // que é gerido à parte em GetClientsPagedAsync/Clients.vue.
    public async Task<PagedResultDTO<UserModel>> GetTeamPagedAsync(int page, int pageSize, string? search, string? role)
    {
        var query = _context.AppUsers.Where(u => u.Role != "client");

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role == role);
        }

        var total = await query.CountAsync();

        var data = await query
            .OrderBy(u => u.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDTO<UserModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
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

    public async Task<UserModel> UpdateAsync(UserModel user)
    {
        _context.AppUsers.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
}