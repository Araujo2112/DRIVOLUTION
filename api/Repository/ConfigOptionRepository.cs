using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir as opções de configuração dos modelos de veículo
public class ConfigOptionRepository : IConfigOptionRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ConfigOptionRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as opções de configuração
    public async Task<IEnumerable<ConfigOptionModel>> GetAll() => 
        await _context.Set<ConfigOptionModel>().ToListAsync();

    // Procura uma opção pelo seu ID
    public async Task<ConfigOptionModel?> GetById(int id) => 
        await _context.Set<ConfigOptionModel>().FindAsync(id);

    // Devolve todas as opções pertencentes a uma configuração
    public async Task<IEnumerable<ConfigOptionModel>> GetByConfigId(int configId) =>
        await _context.Set<ConfigOptionModel>()
            .Where(o => o.ConfigId == configId)
            .ToListAsync();

    // Procura a opção definida como valor por defeito de uma configuração
    public async Task<ConfigOptionModel?> GetDefaultByConfigId(int configId) =>
        await _context.Set<ConfigOptionModel>()
            .FirstOrDefaultAsync(o => o.ConfigId == configId && o.IsDefault);

    // Cria uma nova opção de configuração
    public async Task<ConfigOptionModel> Create(ConfigOptionModel entity)
    {
        _context.Set<ConfigOptionModel>().Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza uma opção existente
    public async Task Update(ConfigOptionModel entity)
    {
        _context.Entry(entity).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    // Remove uma opção de configuração
    public async Task Delete(int id)
    {
        var entity = await GetById(id);

        // Só remove se a opção existir
        if (entity != null)
        {
            _context.Set<ConfigOptionModel>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se uma opção existe
    public async Task<bool> Exists(int id) => 
        await _context.Set<ConfigOptionModel>().AnyAsync(e => e.Id == id);

    // Devolve todas as opções marcadas como valor por defeito
    public async Task<IEnumerable<ConfigOptionModel>> GetDefaultsByConfigId(int configId) =>
        await _context.Set<ConfigOptionModel>()
            .Where(o => o.ConfigId == configId && o.IsDefault)
            .ToListAsync();
}