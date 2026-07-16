using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir os utilizadores da plataforma
public class UserRepository : IUserRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Paginação restrita a role="client" — só usada pela página de gestão de
    // Clientes. A Equipa (admin/manager/operator) usa GetTeamPagedAsync, abaixo.
    public async Task<PagedResultDTO<UserModel>> GetClientsPagedAsync(int page, int pageSize, string? search)
    {
        // Obtém apenas os utilizadores com perfil de cliente
        var query = _context.AppUsers.Where(u => u.Role == "client");

        // Aplica pesquisa por nome, caso tenha sido fornecida
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(s));
        }

        // Conta o número total de resultados
        var total = await query.CountAsync();

        // Obtém apenas os registos da página pretendida
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
        // Obtém apenas utilizadores da equipa interna
        var query = _context.AppUsers.Where(u => u.Role != "client");

        // Aplica pesquisa por nome, caso tenha sido fornecida
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.Name.ToLower().Contains(s));
        }

        // Filtra por perfil, caso indicado
        if (!string.IsNullOrWhiteSpace(role))
        {
            query = query.Where(u => u.Role == role);
        }

        // Conta o número total de resultados
        var total = await query.CountAsync();

        // Obtém apenas os registos da página pretendida
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

    // Procura um utilizador através do endereço de email
    public async Task<UserModel?> GetByEmailAsync(string email)
        => await _context.AppUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

    // Procura um utilizador pelo seu ID
    public async Task<UserModel?> GetByIdAsync(int id)
        => await _context.AppUsers.FindAsync(id);

    // Devolve todos os utilizadores ordenados pelo nome
    public async Task<IEnumerable<UserModel>> GetAllAsync()
        => await _context.AppUsers
            .OrderBy(u => u.Name)
            .ToListAsync();

    // Cria um novo utilizador
    public async Task<UserModel> CreateAsync(UserModel user)
    {
        _context.AppUsers.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // Atualiza um utilizador existente
    public async Task<UserModel> UpdateAsync(UserModel user)
    {
        _context.AppUsers.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }
}