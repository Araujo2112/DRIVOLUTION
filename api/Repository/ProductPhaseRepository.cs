using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class ProductPhaseRepository : IProductPhaseRepository
{
    private readonly ApplicationDbContext _context;
    public ProductPhaseRepository(ApplicationDbContext context) => _context = context;

    public async Task<IEnumerable<ProductPhaseModel>> GetByProduct(int productId) =>
        await _context.ProductPhases
            .Where(pp => pp.ProductId == productId)
            .Include(pp => pp.ManufacturingPhase)
            .Include(pp => pp.Workstation)
            .OrderBy(pp => pp.DatetimeIni)
            .ToListAsync();

    public async Task<ProductPhaseModel?> GetCurrentByProduct(int productId) =>
        await _context.ProductPhases
            .Where(pp => pp.ProductId == productId && pp.DatetimeEnd == null)
            .Include(pp => pp.ManufacturingPhase)
            .Include(pp => pp.Workstation)
            .OrderByDescending(pp => pp.DatetimeIni)
            .FirstOrDefaultAsync();

    public async Task<List<ProductPhaseModel>> GetCurrentByProducts(List<int> productIds) =>
        await _context.ProductPhases
            .Where(pp => productIds.Contains(pp.ProductId) && pp.DatetimeEnd == null)
            .Include(pp => pp.ManufacturingPhase)
            .Include(pp => pp.Workstation)
            .ToListAsync();
   
    public async Task<IEnumerable<ProductPhaseModel>> GetAllOpenByProduct(int productId) =>
        await _context.ProductPhases
            .Where(pp => pp.ProductId == productId && pp.DatetimeEnd == null)
            .OrderBy(pp => pp.DatetimeIni)
            .ToListAsync();

    public async Task<ProductPhaseModel?> GetById(int id) =>
        await _context.ProductPhases.FindAsync(id);

    public async Task<ProductPhaseModel> Create(ProductPhaseModel entity)
    {
        // Garantir UTC antes de inserir
        entity.DatetimeIni = DateTime.SpecifyKind(entity.DatetimeIni, DateTimeKind.Utc);
        _context.ProductPhases.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task Update(ProductPhaseModel entity)
    {
        // Fixar Kind=Utc em todos os campos DateTime antes de guardar
        entity.DatetimeIni = DateTime.SpecifyKind(entity.DatetimeIni, DateTimeKind.Utc);
        if (entity.DatetimeEnd.HasValue)
            entity.DatetimeEnd = DateTime.SpecifyKind(entity.DatetimeEnd.Value, DateTimeKind.Utc);

        _context.ProductPhases.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProductPhaseModel>> GetOpenPhasesWithPhaseInfoAsync() =>
    await _context.ProductPhases
        .Where(pp => pp.DatetimeEnd == null)
        .Include(pp => pp.ManufacturingPhase)
        .Include(pp => pp.Product)
        .ToListAsync();

    public async Task<ProductPhaseModel?> GetByIdAsync(int id) =>
        await _context.ProductPhases
            .Include(pp => pp.ManufacturingPhase)
            .FirstOrDefaultAsync(pp => pp.Id == id);

    public async Task<ProductPhaseModel?> GetCurrentOpenByProductionLine(int productionLineId) =>
        await _context.ProductPhases
            .Where(pp => pp.DatetimeEnd == null && pp.Workstation.ProductionLineId == productionLineId)
            .Include(pp => pp.ManufacturingPhase)
            .Include(pp => pp.Workstation)
            .Include(pp => pp.Product)
            .OrderByDescending(pp => pp.DatetimeIni)
            .FirstOrDefaultAsync();

    public async Task<IEnumerable<ProductPhaseModel>> GetAllOpenByProductionLine(int productionLineId) =>
        await _context.ProductPhases
            .Where(pp => pp.DatetimeEnd == null && pp.Workstation.ProductionLineId == productionLineId)
            .Include(pp => pp.ManufacturingPhase)
            .Include(pp => pp.Workstation)
            .Include(pp => pp.Product)
            .OrderBy(pp => pp.DatetimeIni)
            .ToListAsync();
}