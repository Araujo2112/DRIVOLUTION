using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.QualityCheck;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class QualityCheckRepository : IQualityCheckRepository
{
    private readonly ApplicationDbContext _context;
    public QualityCheckRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<QualityCheckModel>> GetByProduct(int productId) =>
        await _context.QualityChecks.Where(qc => qc.ProductId == productId).Include(qc => qc.ManufacturingPhase).ToListAsync();
    public async Task<QualityCheckModel?> GetById(int id) => await _context.QualityChecks.FindAsync(id);
    public async Task<QualityCheckModel> Create(QualityCheckModel entity)
    {
        _context.QualityChecks.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(QualityCheckModel entity)
    {
        _context.QualityChecks.Update(entity);
        await _context.SaveChangesAsync();
    }
}
