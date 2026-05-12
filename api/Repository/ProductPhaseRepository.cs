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
        await _context.ProductPhases.Where(pp => pp.ProductId == productId).Include(pp => pp.ManufacturingPhase).Include(pp => pp.Workstation).OrderBy(pp => pp.DatetimeIni).ToListAsync();
    public async Task<ProductPhaseModel?> GetCurrentByProduct(int productId) =>
        await _context.ProductPhases.Where(pp => pp.ProductId == productId && pp.DatetimeEnd == null).Include(pp => pp.ManufacturingPhase).Include(pp => pp.Workstation).FirstOrDefaultAsync();
    public async Task<ProductPhaseModel?> GetById(int id) => await _context.ProductPhases.FindAsync(id);
    public async Task<ProductPhaseModel> Create(ProductPhaseModel entity)
    {
        _context.ProductPhases.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(ProductPhaseModel entity)
    {
        _context.ProductPhases.Update(entity);
        await _context.SaveChangesAsync();
    }
}
