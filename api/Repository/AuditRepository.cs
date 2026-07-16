using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por consultar os registos de auditoria do sistema
public class AuditRepository : IAuditRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public AuditRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Devolve uma lista paginada de registos de auditoria,
    // permitindo filtrar por entidade, utilizador e ação
    public async Task<PagedResultDTO<AuditLogModel>> GetPagedAsync(int page, int pageSize, string? entity, int? userId, string? action)
    {
        // Começa por construir a query base
        var query = _context.AuditLogs.AsQueryable();

        // Aplica filtro pela entidade, caso exista
        if (!string.IsNullOrWhiteSpace(entity))
            query = query.Where(a => a.Entity == entity);

        // Aplica filtro pelo utilizador, caso tenha sido indicado
        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        // Aplica filtro pela ação realizada
        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action == action);

        // Conta o número total de resultados antes da paginação
        var total = await query.CountAsync();

        // Obtém apenas os registos da página pretendida
        var data = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Devolve o resultado paginado
        return new PagedResultDTO<AuditLogModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Lista de utilizadores distintos que já geraram logs — usada para popular
    // o dropdown de filtro sem depender da página atual carregada.
    public async Task<IEnumerable<(int UserId, string UserName)>> GetDistinctUsersAsync()
    {
        // Obtém apenas combinações únicas de utilizador
        var users = await _context.AuditLogs
            .Select(a => new { a.UserId, a.UserName })
            .Distinct()
            .OrderBy(a => a.UserName)
            .ToListAsync();

        // Converte o resultado para uma lista de tuplos
        return users.Select(u => (u.UserId, u.UserName));
    }

    // Devolve todos os registos de auditoria
    public async Task<IEnumerable<AuditLogModel>> GetAllAsync()
        => await _context.AuditLogs
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    // Devolve todos os registos associados a uma determinada entidade
    public async Task<IEnumerable<AuditLogModel>> GetByEntityAsync(string entity)
        => await _context.AuditLogs
            .Where(a => a.Entity == entity)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();

    // Devolve todos os registos criados por um determinado utilizador
    public async Task<IEnumerable<AuditLogModel>> GetByUserAsync(int userId)
        => await _context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync();
}