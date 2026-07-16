using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir os materiais utilizados na produção
public class MaterialRepository : IMaterialRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public MaterialRepository(ApplicationDbContext context) => _context = context;

    // Devolve todos os materiais
    public async Task<IEnumerable<MaterialModel>> GetAll() =>
        await _context.Materials.ToListAsync();

    // Procura um material pelo seu ID
    public async Task<MaterialModel?> GetById(int id) =>
        await _context.Materials.FindAsync(id);

    // Cria um novo material
    public async Task<MaterialModel> Create(MaterialModel entity)
    {
        _context.Materials.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza um material existente
    public async Task Update(MaterialModel entity)
    {
        _context.Materials.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Remove um material
    public async Task Delete(int id)
    {
        var entity = await _context.Materials.FindAsync(id);

        // Só remove se o material existir
        if (entity != null)
        {
            _context.Materials.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se um material existe
    public async Task<bool> Exists(int id) =>
        await _context.Materials.AnyAsync(m => m.Id == id);
}