using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;
namespace Drivolution.Repository;
public class SupportRepository : ISupportRepository
{
    private readonly ApplicationDbContext _context;
    public SupportRepository(ApplicationDbContext context) => _context = context;

    public async Task<PagedResultDTO<SupportPagedDTO>> GetPaged(
        int page, int pageSize, string? search, int? productionLineId, bool? occupied)
    {
        // current = SupportedProduct ativo (DatetimeEnd == null) para cada suporte
        var baseQuery =
            from s in _context.Supports
            join pl in _context.ProductionLines on s.ProductionLineId equals pl.Id
            select new
            {
                Support = s,
                LineName = pl.Name,
                Current = _context.SupportedProducts
                    .Where(sp => sp.SupportId == s.Id && sp.DatetimeEnd == null)
                    .OrderByDescending(sp => sp.DatetimeIni)
                    .Select(sp => new { sp.ProductId, sp.Product!.SerialNumber, ModelName = sp.Product!.CarModel!.Name })
                    .FirstOrDefault()
            };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            baseQuery = baseQuery.Where(x =>
                (x.Support.RfidTag != null && x.Support.RfidTag.ToLower().Contains(s)) ||
                (x.Support.Type != null && x.Support.Type.ToLower().Contains(s)));
        }

        if (productionLineId.HasValue)
            baseQuery = baseQuery.Where(x => x.Support.ProductionLineId == productionLineId.Value);

        if (occupied.HasValue)
            baseQuery = occupied.Value
                ? baseQuery.Where(x => x.Current != null)
                : baseQuery.Where(x => x.Current == null);

        var total = await baseQuery.CountAsync();

        var data = await baseQuery
            .OrderBy(x => x.LineName).ThenBy(x => x.Support.Type)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = data.Select(x => new SupportPagedDTO(
            x.Support.Id,
            x.Support.ProductionLineId,
            x.LineName,
            x.Support.RfidTag,
            x.Support.Type,
            x.Current != null,
            x.Current?.ProductId,
            x.Current?.SerialNumber,
            x.Current?.ModelName
        ));

        return new PagedResultDTO<SupportPagedDTO>
        {
            Data = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<SupportModel>> GetAll() =>
        await _context.Supports.Include(s => s.ProductionLine).ToListAsync();
    public async Task<SupportModel?> GetById(int id) =>
        await _context.Supports.Include(s => s.ProductionLine).FirstOrDefaultAsync(s => s.Id == id);
    public async Task<SupportModel?> GetByRfidTag(string rfidTag) =>
        await _context.Supports.FirstOrDefaultAsync(s => s.RfidTag == rfidTag);
    public async Task<SupportModel> Create(SupportModel entity)
    {
        _context.Supports.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(SupportModel entity)
    {
        _context.Supports.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.Supports.FindAsync(id);
        if (entity != null) { _context.Supports.Remove(entity); await _context.SaveChangesAsync(); }
    }
    public async Task<bool> Exists(int id) => await _context.Supports.AnyAsync(s => s.Id == id);
}