using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Microsoft.EntityFrameworkCore;
using Drivolution.Repository.Interface;

namespace Drivolution.Repository;

// Repository responsável por todas as operações sobre os alertas
public class AlertRepository : IAlertRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public AlertRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Devolve uma lista paginada de alertas, permitindo filtrar por tipo e estado
    public async Task<PagedResultDTO<AlertModel>> GetPagedAsync(int page, int pageSize, string? type, string? status)
    {
        // Começa a construir a query incluindo o produto e a fase associados
        var query = _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .AsQueryable();

        // Aplica filtro por tipo, caso tenha sido fornecido
        if (!string.IsNullOrWhiteSpace(type))
            query = query.Where(a => a.Type == type);

        // Aplica filtro por estado, caso tenha sido fornecido
        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(a => a.Status == status);

        // Conta o número total de resultados antes da paginação
        var total = await query.CountAsync();

        // Obtém apenas os registos da página pedida
        var data = await query
            .OrderByDescending(a => a.TriggeredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Devolve o resultado paginado
        return new PagedResultDTO<AlertModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Devolve todos os alertas existentes
    public async Task<IEnumerable<AlertModel>> GetAllAsync()
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

    // Devolve apenas os alertas que ainda estão abertos
    public async Task<IEnumerable<AlertModel>> GetOpenAsync()
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .Where(a => a.Status == "open")
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

    // Verifica se já existe um alerta aberto ou reconhecido
    // para uma determinada fase do produto
    public async Task<bool> ExistsOpenForPhaseAsync(int productPhaseId, string type)
        => await _context.Alerts
            .AnyAsync(a => a.ProductPhaseId == productPhaseId
                        && a.Type == type
                        && (a.Status == "open" || a.Status == "acknowledged"));

    // Procura um alerta pelo seu ID
    public async Task<AlertModel?> GetByIdAsync(int id)
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .FirstOrDefaultAsync(a => a.Id == id);

    // Cria um novo alerta
    public async Task<AlertModel> CreateAsync(AlertModel alert)
    {
        // Adiciona o alerta à base de dados
        _context.Alerts.Add(alert);

        // Guarda as alterações
        await _context.SaveChangesAsync();

        return alert;
    }

    // Atualiza um alerta existente
    public async Task<AlertModel> UpdateAsync(AlertModel alert)
    {
        // Atualiza o registo
        _context.Alerts.Update(alert);

        // Guarda as alterações
        await _context.SaveChangesAsync();

        return alert;
    }

    // Devolve todos os alertas abertos ou reconhecidos
    // de um determinado tipo
    public async Task<IEnumerable<AlertModel>> GetOpenByTypeAsync(string type)
        => await _context.Alerts
            .Include(a => a.Product)
            .Include(a => a.ProductPhase)
            .Where(a => (a.Status == "open" || a.Status == "acknowledged") && a.Type == type)
            .OrderByDescending(a => a.TriggeredAt)
            .ToListAsync();

    // Devolve os alertas pendentes de um determinado produto e tipo
    public async Task<IEnumerable<AlertModel>> GetPendingByProductAndTypeAsync(int productId, string type)
        => await _context.Alerts
            .Where(a => a.ProductId == productId
                    && a.Type == type
                    && (a.Status == "open" || a.Status == "acknowledged"))
            .ToListAsync();
}