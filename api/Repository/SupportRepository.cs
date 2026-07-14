using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Implementa o contrato definido em ISupportRepository
public class SupportRepository : ISupportRepository
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o contexto da base de dados
    public SupportRepository(ApplicationDbContext context)
        => _context = context;

    // Devolve uma lista paginada de suportes,
    // permitindo pesquisar e filtrar por linha e ocupação
    public async Task<PagedResultDTO<SupportPagedDTO>> GetPaged(
        int page,
        int pageSize,
        string? search,
        int? productionLineId,
        bool? occupied)
    {
        // current = SupportedProduct ativo (DatetimeEnd == null) para cada suporte
        var baseQuery =
            from s in _context.Supports

            // Junta cada suporte à respetiva linha de produção
            join pl in _context.ProductionLines
                on s.ProductionLineId equals pl.Id

            // Cria um objeto temporário com os dados necessários
            select new
            {
                // Dados do suporte
                Support = s,

                // Nome da linha de produção
                LineName = pl.Name,

                // Procura o produto que está atualmente neste suporte
                Current = _context.SupportedProducts

                    // Filtra pelo suporte e por registos ainda ativos
                    .Where(sp =>
                        sp.SupportId == s.Id &&
                        sp.DatetimeEnd == null
                    )

                    // Se existirem vários registos abertos,
                    // escolhe o mais recente
                    .OrderByDescending(sp => sp.DatetimeIni)

                    // Seleciona apenas os dados necessários do produto atual
                    .Select(sp => new
                    {
                        sp.ProductId,
                        sp.Product!.SerialNumber,

                        // Nome do modelo do produto
                        ModelName = sp.Product!.CarModel!.Name
                    })

                    // Devolve o primeiro registo ou null
                    .FirstOrDefault()
            };

        // Se foi enviado um texto de pesquisa
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Remove espaços e converte para minúsculas
            var s = search.Trim().ToLower();

            // Pesquisa pela tag RFID ou pelo tipo do suporte
            baseQuery = baseQuery.Where(x =>
                (
                    x.Support.RfidTag != null &&
                    x.Support.RfidTag
                        .ToLower()
                        .Contains(s)
                )
                ||
                (
                    x.Support.Type != null &&
                    x.Support.Type
                        .ToLower()
                        .Contains(s)
                )
            );
        }

        // Se foi escolhida uma linha de produção,
        // devolve apenas os suportes dessa linha
        if (productionLineId.HasValue)
        {
            baseQuery = baseQuery.Where(x =>
                x.Support.ProductionLineId ==
                productionLineId.Value
            );
        }

        // Se foi indicado se o suporte deve estar ocupado ou livre
        if (occupied.HasValue)
        {
            baseQuery = occupied.Value

                // occupied = true:
                // devolve apenas suportes com produto atual
                ? baseQuery.Where(x => x.Current != null)

                // occupied = false:
                // devolve apenas suportes sem produto atual
                : baseQuery.Where(x => x.Current == null);
        }

        // Conta o número total de suportes que cumprem os filtros
        var total = await baseQuery.CountAsync();

        // Obtém apenas os suportes da página pedida
        var data = await baseQuery

            // Ordena primeiro pelo nome da linha
            .OrderBy(x => x.LineName)

            // Dentro da mesma linha, ordena pelo tipo do suporte
            .ThenBy(x => x.Support.Type)

            // Ignora os registos das páginas anteriores
            .Skip((page - 1) * pageSize)

            // Limita os resultados ao tamanho da página
            .Take(pageSize)

            // Executa a consulta
            .ToListAsync();

        // Converte os resultados para SupportPagedDTO
        var dtos = data.Select(x => new SupportPagedDTO(
            x.Support.Id,
            x.Support.ProductionLineId,
            x.LineName,
            x.Support.RfidTag,
            x.Support.Type,

            // O suporte está ocupado se existir um produto atual
            x.Current != null,

            // Dados opcionais do produto atual
            x.Current?.ProductId,
            x.Current?.SerialNumber,
            x.Current?.ModelName
        ));

        // Devolve os dados juntamente com a informação da paginação
        return new PagedResultDTO<SupportPagedDTO>
        {
            Data = dtos,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Devolve todos os suportes
    // e inclui a respetiva linha de produção
    public async Task<IEnumerable<SupportModel>> GetAll() =>
        await _context.Supports
            .Include(s => s.ProductionLine)
            .ToListAsync();

    // Procura um suporte pelo ID
    // e inclui a respetiva linha de produção
    public async Task<SupportModel?> GetById(int id) =>
        await _context.Supports
            .Include(s => s.ProductionLine)
            .FirstOrDefaultAsync(s => s.Id == id);

    // Procura um suporte através da sua tag RFID
    public async Task<SupportModel?> GetByRfidTag(string rfidTag) =>
        await _context.Supports
            .FirstOrDefaultAsync(s =>
                s.RfidTag == rfidTag
            );

    // Cria um novo suporte
    public async Task<SupportModel> Create(SupportModel entity)
    {
        // Adiciona o suporte ao contexto
        _context.Supports.Add(entity);

        // Guarda o novo suporte na base de dados
        await _context.SaveChangesAsync();

        // Devolve a entidade criada
        return entity;
    }

    // Atualiza um suporte existente
    public async Task Update(SupportModel entity)
    {
        // Marca o suporte como atualizado
        _context.Supports.Update(entity);

        // Guarda as alterações na base de dados
        await _context.SaveChangesAsync();
    }

    // Elimina um suporte através do ID
    public async Task Delete(int id)
    {
        // Procura o suporte pelo ID
        var entity = await _context.Supports.FindAsync(id);

        // Só elimina se o suporte existir
        if (entity != null)
        {
            _context.Supports.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se existe um suporte com o ID indicado
    public async Task<bool> Exists(int id) =>
        await _context.Supports
            .AnyAsync(s => s.Id == id);
}