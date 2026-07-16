using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir as workstations da linha de produção
public class WorkstationRepository : IWorkstationRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public WorkstationRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as workstations, incluindo a linha de produção
    // e a fase de fabrico associadas
    public async Task<IEnumerable<WorkstationModel>> GetAll() =>
        await _context.Workstations
            .Include(w => w.ProductionLine)
            .Include(w => w.ManufacturingPhase)
            .ToListAsync();

    // Devolve todas as workstations pertencentes a uma linha de produção
    public async Task<IEnumerable<WorkstationModel>> GetByProductionLine(int productionLineId) =>
        await _context.Workstations
            .Include(w => w.ManufacturingPhase)
            .Where(w => w.ProductionLineId == productionLineId)
            .ToListAsync();

    // Procura uma workstation pelo seu ID,
    // incluindo a linha de produção e a fase associadas
    public async Task<WorkstationModel?> GetById(int id) =>
        await _context.Workstations
            .Include(w => w.ProductionLine)
            .Include(w => w.ManufacturingPhase)
            .FirstOrDefaultAsync(w => w.Id == id);

    // Cria uma nova workstation
    public async Task<WorkstationModel> Create(WorkstationModel entity)
    {
        // Adiciona a workstation à base de dados
        _context.Workstations.Add(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();

        return entity;
    }

    // Atualiza uma workstation existente
    public async Task Update(WorkstationModel entity)
    {
        // Marca a entidade como alterada
        _context.Workstations.Update(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();
    }

    // Remove uma workstation pelo seu ID
    public async Task Delete(int id)
    {
        // Procura a workstation
        var entity = await _context.Workstations.FindAsync(id);

        // Se existir, remove-a da base de dados
        if (entity != null)
        {
            _context.Workstations.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se existe uma workstation com o ID indicado
    public async Task<bool> Exists(int id) =>
        await _context.Workstations.AnyAsync(w => w.Id == id);
}