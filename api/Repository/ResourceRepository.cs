using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir os recursos da fábrica
public class ResourceRepository : IResourceRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ResourceRepository(ApplicationDbContext context) => _context = context;

    // Devolve todos os recursos
    public async Task<IEnumerable<ResourceModel>> GetAll() =>
        await _context.Resources.ToListAsync();

    // Procura um recurso pelo seu ID
    public async Task<ResourceModel?> GetById(int id) =>
        await _context.Resources.FindAsync(id);

    // Cria um novo recurso
    public async Task<ResourceModel> Create(ResourceModel entity)
    {
        _context.Resources.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza um recurso existente
    public async Task Update(ResourceModel entity)
    {
        _context.Resources.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Remove um recurso
    public async Task Delete(int id)
    {
        var entity = await _context.Resources.FindAsync(id);

        // Só remove se o recurso existir
        if (entity != null)
        {
            _context.Resources.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se um recurso existe
    public async Task<bool> Exists(int id) =>
        await _context.Resources.AnyAsync(r => r.Id == id);
}