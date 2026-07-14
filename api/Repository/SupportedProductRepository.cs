using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Implementa o contrato definido em ISupportedProductRepository
public class SupportedProductRepository : ISupportedProductRepository
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o contexto da base de dados
    public SupportedProductRepository(ApplicationDbContext context)
        => _context = context;

    // Devolve todo o histórico de produtos associados a um suporte
    public async Task<IEnumerable<SupportedProductModel>> GetBySupport(
        int supportId) =>
        await _context.SupportedProducts

            // Filtra apenas os registos do suporte indicado
            .Where(sp => sp.SupportId == supportId)

            // Inclui os dados do produto associado
            .Include(sp => sp.Product)

                // Inclui também o modelo do carro
                .ThenInclude(p => p!.CarModel)

            // Ordena do registo mais recente para o mais antigo
            .OrderByDescending(sp => sp.DatetimeIni)

            // Executa a consulta e devolve uma lista
            .ToListAsync();

    // Devolve o produto que está atualmente num suporte
    public async Task<SupportedProductModel?> GetCurrentBySupport(
        int supportId) =>
        await _context.SupportedProducts

            // Procura um registo do suporte que ainda não terminou
            .Where(sp =>
                sp.SupportId == supportId &&
                sp.DatetimeEnd == null
            )

            // Inclui o produto associado
            .Include(sp => sp.Product)

                // Inclui o modelo do produto
                .ThenInclude(p => p!.CarModel)

            // Devolve o primeiro registo encontrado ou null
            .FirstOrDefaultAsync();

    // Procura uma associação entre suporte e produto pelo seu ID
    public async Task<SupportedProductModel?> GetById(int id) =>
        await _context.SupportedProducts

            // Inclui o produto associado
            .Include(sp => sp.Product)

                // Inclui o modelo do produto
                .ThenInclude(p => p!.CarModel)

            // Procura o registo com o ID indicado
            .FirstOrDefaultAsync(sp => sp.Id == id);

    // Cria uma nova associação entre um suporte e um produto
    public async Task<SupportedProductModel> Create(
        SupportedProductModel entity)
    {
        // Adiciona a associação ao contexto
        _context.SupportedProducts.Add(entity);

        // Guarda a associação na base de dados
        await _context.SaveChangesAsync();

        // Devolve a entidade criada
        return entity;
    }

    // Atualiza uma associação existente
    public async Task Update(SupportedProductModel entity)
    {
        // Marca a entidade como atualizada
        _context.SupportedProducts.Update(entity);

        // Guarda as alterações na base de dados
        await _context.SaveChangesAsync();
    }
}