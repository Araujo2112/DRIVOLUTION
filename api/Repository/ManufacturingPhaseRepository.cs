using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir as fases de fabrico
public class ManufacturingPhaseRepository : IManufacturingPhaseRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ManufacturingPhaseRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as fases de fabrico
    public async Task<IEnumerable<ManufacturingPhaseModel>> GetAll() =>
        await _context.ManufacturingPhases.ToListAsync();

    // Procura uma fase de fabrico pelo seu ID
    public async Task<ManufacturingPhaseModel?> GetById(int id) =>
        await _context.ManufacturingPhases.FindAsync(id);

    // Cria uma nova fase de fabrico
    public async Task<ManufacturingPhaseModel> Create(ManufacturingPhaseModel entity)
    {
        _context.ManufacturingPhases.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza uma fase de fabrico existente
    public async Task Update(ManufacturingPhaseModel entity)
    {
        _context.ManufacturingPhases.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Remove uma fase de fabrico
    public async Task Delete(int id)
    {
        var entity = await _context.ManufacturingPhases.FindAsync(id);

        // Só remove se a fase existir
        if (entity != null)
        {
            _context.ManufacturingPhases.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se uma fase de fabrico existe
    public async Task<bool> Exists(int id) =>
        await _context.ManufacturingPhases.AnyAsync(mp => mp.Id == id);
}