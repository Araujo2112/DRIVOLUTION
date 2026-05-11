using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.PhaseSequence;
using Microsoft.EntityFrameworkCore;
namespace ApiTexPact.Repository;
public class PhaseSequenceRepository : IPhaseSequenceRepository
{
    private readonly ApplicationDbContext _context;
    public PhaseSequenceRepository(ApplicationDbContext context) => _context = context;
    public async Task<IEnumerable<PhaseSequenceModel>> GetByModel(int modelId) =>
        await _context.PhaseSequences.Where(ps => ps.ModelId == modelId).Include(ps => ps.ManufacturingPhase).OrderBy(ps => ps.Order).ToListAsync();
    public async Task<PhaseSequenceModel?> GetById(int id) => await _context.PhaseSequences.FindAsync(id);
    public async Task<PhaseSequenceModel> Create(PhaseSequenceModel entity)
    {
        _context.PhaseSequences.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }
    public async Task Update(PhaseSequenceModel entity)
    {
        _context.PhaseSequences.Update(entity);
        await _context.SaveChangesAsync();
    }
    public async Task Delete(int id)
    {
        var entity = await _context.PhaseSequences.FindAsync(id);
        if (entity != null) { _context.PhaseSequences.Remove(entity); await _context.SaveChangesAsync(); }
    }
}
