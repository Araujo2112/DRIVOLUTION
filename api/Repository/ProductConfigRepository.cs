using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir as configurações associadas a cada produto
public class ProductConfigRepository : IProductConfigRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ProductConfigRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as configurações associadas a um produto
    public async Task<IEnumerable<ProductConfigModel>> GetByProduct(int productId) =>
        await _context.ProductConfigs
            .Where(pc => pc.ProductId == productId)
            .Include(pc => pc.ConfigOption)
            .ToListAsync();

    // Cria uma nova configuração para um produto
    public async Task<ProductConfigModel> Create(ProductConfigModel entity)
    {
        _context.ProductConfigs.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    // Atualiza uma configuração de um produto
    public async Task Update(ProductConfigModel entity)
    {
        _context.ProductConfigs.Update(entity);
        await _context.SaveChangesAsync();
    }

    // Remove uma configuração de um produto
    public async Task Delete(int id)
    {
        var entity = await _context.ProductConfigs.FindAsync(id);

        // Só remove se a configuração existir
        if (entity != null)
        {
            _context.ProductConfigs.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Procura uma configuração específica de um produto através da opção selecionada
    public async Task<ProductConfigModel?> GetByProductAndOption(int productId, int configOptionId) =>
        await _context.ProductConfigs
            .FirstOrDefaultAsync(pc => pc.ProductId == productId && pc.ConfigOptionId == configOptionId);

    // Devolve todas as opções de uma determinada configuração associadas a um produto
    public async Task<IEnumerable<ProductConfigModel>> GetByProductAndConfig(int productId, int configId) =>
        await _context.ProductConfigs
            .Include(pc => pc.ConfigOption)
            .Where(pc => pc.ProductId == productId && pc.ConfigOption.ConfigId == configId)
            .ToListAsync();

    // Devolve as configurações de vários produtos em simultâneo
    public async Task<List<ProductConfigModel>> GetByProducts(List<int> productIds) =>
        await _context.ProductConfigs
            .Where(pc => productIds.Contains(pc.ProductId))
            .ToListAsync();
}