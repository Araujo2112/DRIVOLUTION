using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir as linhas de produção
public class ProductionLineRepository : IProductionLineRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ProductionLineRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as linhas de produção, incluindo as respetivas workstations
    public async Task<IEnumerable<ProductionLineModel>> GetAll() =>
        await _context.ProductionLines
            .Include(p => p.Workstations)
            .ToListAsync();

    // Procura uma linha de produção pelo seu ID, incluindo as respetivas workstations
    public async Task<ProductionLineModel?> GetById(int id) =>
        await _context.ProductionLines
            .Include(p => p.Workstations)
            .FirstOrDefaultAsync(p => p.Id == id);

    // Cria uma nova linha de produção
    public async Task<ProductionLineModel> Create(ProductionLineModel entity)
    {
        _context.ProductionLines.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza uma linha de produção existente
    public async Task Update(ProductionLineModel entity)
    {
        _context.ProductionLines.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Remove uma linha de produção
    public async Task Delete(int id)
    {
        var entity = await _context.ProductionLines.FindAsync(id);

        // Só remove se a linha de produção existir
        if (entity != null)
        {
            _context.ProductionLines.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se uma linha de produção existe
    public async Task<bool> Exists(int id) =>
        await _context.ProductionLines.AnyAsync(p => p.Id == id);
}