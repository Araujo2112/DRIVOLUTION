using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por todas as operações relacionadas com os modelos de veículos
public class CarModelRepository : ICarModelRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public CarModelRepository(ApplicationDbContext context) => _context = context;

    // Devolve todos os modelos de veículos existentes
    public async Task<IEnumerable<CarModelModel>> GetAll()
        => await _context.CarModels.ToListAsync();

    // Procura um modelo de veículo pelo seu ID
    public async Task<CarModelModel?> GetById(int id)
        => await _context.CarModels.FindAsync(id);

    // Procura um modelo incluindo toda a sequência de fases de produção
    // e a informação de cada fase associada
    public async Task<CarModelModel?> GetByIdWithPhaseSequence(int id) =>
        await _context.CarModels
            .Include(m => m.PhaseSequences)
            .ThenInclude(ps => ps.ManufacturingPhase)
            .FirstOrDefaultAsync(m => m.Id == id);

    // Devolve todas as configurações pertencentes a um modelo
    public async Task<IEnumerable<ConfigModel>> GetConfigs(int modelId) =>
        await _context.Configs
            .Where(c => c.ModelId == modelId)
            .ToListAsync();

    // Devolve todas as configurações do modelo,
    // incluindo as respetivas opções disponíveis
    public async Task<IEnumerable<ConfigModel>> GetConfigsWithOptions(int modelId) =>
        await _context.Configs
            .Where(c => c.ModelId == modelId)
            .Include(c => c.ConfigOptions)
            .ToListAsync();

    // Cria um novo modelo de veículo
    public async Task<CarModelModel> Create(CarModelModel entity)
    {
        // Adiciona o modelo à base de dados
        _context.CarModels.Add(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();

        return entity;
    }

    // Atualiza um modelo existente
    public async Task Update(CarModelModel entity)
    {
        // Atualiza o registo
        _context.CarModels.Update(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();
    }

    // Remove um modelo de veículo
    public async Task Delete(int id)
    {
        // Procura o modelo pelo ID
        var entity = await _context.CarModels.FindAsync(id);

        // Se existir, remove-o da base de dados
        if (entity != null)
        {
            _context.CarModels.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se existe um modelo com o ID indicado
    public async Task<bool> Exists(int id)
        => await _context.CarModels.AnyAsync(m => m.Id == id);
}