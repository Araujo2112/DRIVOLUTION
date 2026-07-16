using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir a sequência de fases de fabrico de cada modelo de veículo
public class PhaseSequenceRepository : IPhaseSequenceRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public PhaseSequenceRepository(ApplicationDbContext context) => _context = context;

    // Devolve a sequência de fases associada a um modelo de veículo
    public async Task<IEnumerable<PhaseSequenceModel>> GetByModel(int modelId) =>
        await _context.PhaseSequences
            .Where(ps => ps.ModelId == modelId)
            .Include(ps => ps.ManufacturingPhase)
            .OrderBy(ps => ps.Order)
            .ToListAsync();

    // Procura uma sequência de fase pelo seu ID
    public async Task<PhaseSequenceModel?> GetById(int id) =>
        await _context.PhaseSequences.FindAsync(id);

    // Cria uma nova sequência de fase
    public async Task<PhaseSequenceModel> Create(PhaseSequenceModel entity)
    {
        _context.PhaseSequences.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza uma sequência de fase existente
    public async Task Update(PhaseSequenceModel entity)
    {
        _context.PhaseSequences.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Remove uma sequência de fase
    public async Task Delete(int id)
    {
        var entity = await _context.PhaseSequences.FindAsync(id);

        // Só remove se a sequência existir
        if (entity != null)
        {
            _context.PhaseSequences.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}