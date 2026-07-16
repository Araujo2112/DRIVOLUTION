using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir o histórico de localização dos suportes
public class LocalizationHistoryRepository : ILocalizationHistoryRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public LocalizationHistoryRepository(ApplicationDbContext context) => _context = context;

    // Devolve todo o histórico de localização de um suporte
    public async Task<IEnumerable<LocalizationHistoryModel>> GetBySupport(int supportId) =>
        await _context.LocalizationHistories
            .Where(lh => lh.SupportId == supportId)
            .Include(lh => lh.Workstation)
            .OrderByDescending(lh => lh.DatetimeIni)
            .ToListAsync();

    // Devolve a localização atual do suporte (registo ainda não terminado)
    public async Task<LocalizationHistoryModel?> GetCurrentBySupport(int supportId) =>
        await _context.LocalizationHistories
            .Where(lh => lh.SupportId == supportId && lh.DatetimeEnd == null)
            .Include(lh => lh.Workstation)
            .FirstOrDefaultAsync();

    // Cria um novo registo de localização
    public async Task<LocalizationHistoryModel> Create(LocalizationHistoryModel entity)
    {
        _context.LocalizationHistories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza um registo de localização existente
    public async Task Update(LocalizationHistoryModel entity)
    {
        _context.LocalizationHistories.Update(entity);
        await _context.SaveChangesAsync();
    }
}