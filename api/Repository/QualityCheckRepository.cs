using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class QualityCheckRepository : IQualityCheckRepository
{
    private readonly ApplicationDbContext _context;

    public QualityCheckRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<QualityCheckModel>> GetByProduct(int productId) =>
        await _context.QualityChecks
            .Where(qc => qc.ProductId == productId)
            .Include(qc => qc.ManufacturingPhase)
            .ToListAsync();

    public async Task<QualityCheckModel?> GetById(int id) =>
        await _context.QualityChecks.FindAsync(id);

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

    public async Task<bool> HasPassedForProductPhaseAsync(int productId, int manufacturingPhaseId) =>
        await _context.QualityChecks.AnyAsync(qc =>
            qc.ProductId == productId &&
            qc.ManufacturingPhaseId == manufacturingPhaseId &&
            qc.Status == QualityStatus.Passed);
}