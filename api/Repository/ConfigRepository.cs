using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir as configurações dos modelos de veículo
public class ConfigRepository : IConfigRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ConfigRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as configurações existentes
    public async Task<IEnumerable<ConfigModel>> GetAll() => 
        await _context.Set<ConfigModel>().ToListAsync();

    // Procura uma configuração pelo seu ID
    public async Task<ConfigModel?> GetById(int id) => 
        await _context.Set<ConfigModel>().FindAsync(id);

    // Devolve todas as configurações associadas a um modelo de veículo
    public async Task<IEnumerable<ConfigModel>> GetByModelId(int modelId) =>
        await _context.Set<ConfigModel>()
            .Where(c => c.ModelId == modelId)
            .ToListAsync();

    // Cria uma nova configuração
    public async Task<ConfigModel> Create(ConfigModel entity)
    {
        _context.Set<ConfigModel>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza uma configuração existente
    public async Task Update(ConfigModel entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    // Remove uma configuração
    public async Task Delete(int id)
    {
        var entity = await GetById(id);

        // Só remove se a configuração existir
        if (entity != null)
        {
            _context.Set<ConfigModel>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se uma configuração existe
    public async Task<bool> Exists(int id) => 
        await _context.Set<ConfigModel>().AnyAsync(e => e.Id == id);
}