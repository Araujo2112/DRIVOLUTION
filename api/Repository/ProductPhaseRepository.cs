using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

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
}