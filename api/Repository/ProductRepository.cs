using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Implementa o contrato definido em IProductRepository
public class ProductRepository : IProductRepository
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o contexto da base de dados
    public ProductRepository(ApplicationDbContext context)
        => _context = context;

    // Devolve uma lista paginada de produtos,
    // podendo aplicar pesquisa e vários filtros
    public async Task<PagedResultDTO<ProductModel>> GetPaged(
        int page,
        int pageSize,
        string? search,
        int? modelId,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        // Começa a construir a consulta à tabela dos produtos
        var query = _context.Products

            // Inclui os dados do modelo do carro
            .Include(p => p.CarModel)

            // Inclui os dados da ordem de fabrico
            .Include(p => p.ManufacturingOrder)

            // Permite continuar a acrescentar filtros à consulta
            .AsQueryable();

        // Se tiver sido enviado um texto de pesquisa
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Remove espaços no início e no fim
            // e converte o texto para minúsculas
            var s = search.Trim().ToLower();

            // Filtra pelo número de série ou pelo número do lote
            query = query.Where(p =>
                (p.SerialNumber != null &&
                 p.SerialNumber.ToLower().Contains(s))
                ||
                (p.LotNumber != null &&
                 p.LotNumber.ToLower().Contains(s))
            );
        }

        // Se tiver sido enviado um modelo,
        // devolve apenas produtos desse modelo
        if (modelId.HasValue)
        {
            query = query.Where(p =>
                p.ModelId == modelId.Value
            );
        }

        // Se existir uma data inicial,
        // devolve apenas produtos produzidos a partir dessa data
        if (dateFrom.HasValue)
        {
            query = query.Where(p =>
                p.ProductionDate >= dateFrom.Value
            );
        }

        // Se existir uma data final,
        // devolve apenas produtos produzidos até essa data
        if (dateTo.HasValue)
        {
            query = query.Where(p =>
                p.ProductionDate <= dateTo.Value.AddDays(1)
            );
        }

        // Conta o número total de produtos que cumprem os filtros,
        // antes de aplicar a paginação
        var total = await query.CountAsync();

        // Obtém apenas os produtos da página pedida
        var data = await query

            // Ordena dos produtos mais recentes para os mais antigos
            .OrderByDescending(p => p.ProductionDate)

            // Ignora os registos das páginas anteriores
            .Skip((page - 1) * pageSize)

            // Limita o número de resultados ao tamanho da página
            .Take(pageSize)

            // Executa a consulta e transforma o resultado numa lista
            .ToListAsync();

        // Devolve os produtos juntamente com os dados da paginação
        return new PagedResultDTO<ProductModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Procura um produto pelo seu ID
    public async Task<ProductModel?> GetById(int id) =>
        await _context.Products

            // Inclui os dados do modelo do carro
            .Include(p => p.CarModel)

            // Inclui os dados da ordem de fabrico
            .Include(p => p.ManufacturingOrder)

            // Devolve o primeiro produto com o ID indicado,
            // ou null caso não exista
            .FirstOrDefaultAsync(p => p.Id == id);

    // Devolve todos os produtos de uma ordem de fabrico
    public async Task<IEnumerable<ProductModel>>
        GetByManufacturingOrder(int orderId) =>
        await _context.Products

            // Filtra os produtos pela ordem de fabrico
            .Where(p =>
                p.ManufacturingOrderId == orderId
            )

            // Inclui os dados do modelo do carro
            .Include(p => p.CarModel)

            // Executa a consulta e devolve uma lista
            .ToListAsync();

    // Cria um novo produto
    public async Task<ProductModel> Create(ProductModel entity)
    {
        // Adiciona o produto ao contexto
        _context.Products.Add(entity);

        // Guarda o produto na base de dados
        await _context.SaveChangesAsync();

        // Devolve a entidade criada
        return entity;
    }

    // Atualiza um produto existente
    public async Task Update(ProductModel entity)
    {
        // Marca o produto como atualizado
        _context.Products.Update(entity);

        // Guarda as alterações na base de dados
        await _context.SaveChangesAsync();
    }

    // Elimina um produto através do ID
    public async Task Delete(int id)
    {
        // Procura o produto pelo ID
        var entity = await _context.Products.FindAsync(id);

        // Só tenta eliminar se o produto existir
        if (entity != null)
        {
            // Remove o produto do contexto
            _context.Products.Remove(entity);

            // Guarda a eliminação na base de dados
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se existe um produto com o ID indicado
    public async Task<bool> Exists(int id) =>
        await _context.Products.AnyAsync(
            p => p.Id == id
        );
}