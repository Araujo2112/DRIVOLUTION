using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace Drivolution.Repository;
public class LocalizationHistoryRepository : ILocalizationHistoryRepository
{
    private readonly ApplicationDbContext _context;
    public LocalizationHistoryRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<LocalizationHistoryModel>> GetBySupport(int supportId) =>
        await _context.LocalizationHistories.Where(lh => lh.SupportId == supportId).Include(lh => lh.Workstation).OrderByDescending(lh => lh.DatetimeIni).ToListAsync();
    public async Task<LocalizationHistoryModel?> GetCurrentBySupport(int supportId) =>
        await _context.LocalizationHistories.Where(lh => lh.SupportId == supportId && lh.DatetimeEnd == null).Include(lh => lh.Workstation).FirstOrDefaultAsync();
    public async Task<LocalizationHistoryModel> Create(LocalizationHistoryModel entity)
    {
        _context.LocalizationHistories.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(LocalizationHistoryModel entity)
    {
        _context.LocalizationHistories.Update(entity);
        await _context.SaveChangesAsync();
    }
}
