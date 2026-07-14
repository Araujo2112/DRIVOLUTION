using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Implementa o contrato definido em IManufacturingOrderRepository
public class ManufacturingOrderRepository : IManufacturingOrderRepository
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o contexto da base de dados
    public ManufacturingOrderRepository(ApplicationDbContext context)
        => _context = context;

    // Devolve uma lista paginada de ordens de fabrico,
    // podendo aplicar pesquisa e filtros
    public async Task<PagedResultDTO<ManufacturingOrderModel>> GetPaged(
        int page,
        int pageSize,
        string? search,
        string? status,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        // Começa a construir a consulta às ordens de fabrico
        var query = _context.ManufacturingOrders

            // Inclui a encomenda do cliente associada à ordem de fabrico
            .Include(mo => mo.ClientOrder)

                // Inclui também o utilizador/cliente dessa encomenda
                .ThenInclude(c => c.AppUser)

            // Permite continuar a acrescentar filtros à consulta
            .AsQueryable();

        // Se foi enviado um estado diferente de "all",
        // filtra apenas as ordens com esse estado
        if (!string.IsNullOrWhiteSpace(status) &&
            status != "all")
        {
            query = query.Where(mo =>
                mo.Status == status
            );
        }

        // Se existir uma data inicial,
        // devolve ordens iniciadas a partir dessa data
        if (dateFrom.HasValue)
        {
            query = query.Where(mo =>
                mo.StartDate >= dateFrom.Value
            );
        }

        // Se existir uma data final,
        // devolve ordens iniciadas até ao final desse dia
        if (dateTo.HasValue)
        {
            query = query.Where(mo =>
                mo.StartDate <= dateTo.Value.AddDays(1)
            );
        }

        // Se foi enviado texto para pesquisa
        if (!string.IsNullOrWhiteSpace(search))
        {
            // Remove espaços e converte o texto para minúsculas
            var s = search.Trim().ToLower();

            // Pesquisa pelo número da ordem de fabrico
            // ou pelo nome do cliente
            query = query.Where(mo =>
                mo.ManufacturingOrderNumber
                    .ToLower()
                    .Contains(s)
                ||
                (
                    mo.ClientOrder.AppUser != null &&
                    mo.ClientOrder.AppUser.Name
                        .ToLower()
                        .Contains(s)
                )
            );
        }

        // Conta o total de ordens que cumprem os filtros,
        // antes de aplicar a paginação
        var total = await query.CountAsync();

        // Obtém apenas os resultados da página pedida
        var data = await query

            // Ordena da ordem mais recente para a mais antiga
            .OrderByDescending(mo => mo.StartDate)

            // Ignora os registos das páginas anteriores
            .Skip((page - 1) * pageSize)

            // Limita os resultados ao tamanho da página
            .Take(pageSize)

            // Executa a consulta e devolve uma lista
            .ToListAsync();

        // Devolve os dados juntamente com a informação da paginação
        return new PagedResultDTO<ManufacturingOrderModel>
        {
            Data = data,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    // Procura uma ordem de fabrico pelo seu ID
    public async Task<ManufacturingOrderModel?> GetById(int id) =>
        await _context.ManufacturingOrders

            // Inclui a encomenda do cliente
            .Include(mo => mo.ClientOrder)

            // Inclui também o utilizador associado
            .ThenInclude(c => c.AppUser)

            // Devolve a ordem encontrada ou null
            .FirstOrDefaultAsync(mo => mo.Id == id);

    // Procura uma ordem de fabrico com todos os dados relacionados
    public async Task<ManufacturingOrderModel?> GetByIdWithDetails(int id) =>
        await _context.ManufacturingOrders

            // Inclui a encomenda e o cliente
            .Include(mo => mo.ClientOrder)
                .ThenInclude(c => c.AppUser)

            // Inclui os produtos e respetivos modelos
            .Include(mo => mo.Products)
                .ThenInclude(p => p.CarModel)

            // Inclui as configurações de cada produto
            .Include(mo => mo.Products)
                .ThenInclude(p => p.ProductConfigs)

                    // Inclui a opção selecionada
                    .ThenInclude(pc => pc.ConfigOption)

                        // Inclui a configuração a que a opção pertence
                        .ThenInclude(co => co.Config)

            // Inclui as fases de produção de cada produto
            .Include(mo => mo.Products)
                .ThenInclude(p => p.ProductPhases)

                    // Inclui a informação da fase de fabrico
                    .ThenInclude(pp => pp.ManufacturingPhase)

            // Devolve a ordem encontrada ou null
            .FirstOrDefaultAsync(mo => mo.Id == id);

    // Cria uma nova ordem de fabrico
    public async Task<ManufacturingOrderModel> Create(
        ManufacturingOrderModel entity)
    {
        // Adiciona a ordem ao contexto
        _context.ManufacturingOrders.Add(entity);

        // Guarda a ordem na base de dados
        await _context.SaveChangesAsync();

        // Carrega manualmente a encomenda do cliente associada
        await _context.Entry(entity)
            .Reference(mo => mo.ClientOrder)
            .LoadAsync();

        // Se a encomenda existir, carrega também o utilizador
        if (entity.ClientOrder != null)
        {
            await _context.Entry(entity.ClientOrder)
                .Reference(c => c.AppUser)
                .LoadAsync();
        }

        // Devolve a entidade criada já com os dados relacionados carregados
        return entity;
    }

    // Atualiza uma ordem de fabrico existente
    public async Task Update(ManufacturingOrderModel entity)
    {
        // Marca a entidade como atualizada
        _context.ManufacturingOrders.Update(entity);

        // Guarda as alterações na base de dados
        await _context.SaveChangesAsync();
    }

    // Elimina uma ordem de fabrico pelo ID
    public async Task Delete(int id)
    {
        // Procura a ordem através da chave primária
        var entity =
            await _context.ManufacturingOrders.FindAsync(id);

        // Só elimina se a ordem existir
        if (entity != null)
        {
            _context.ManufacturingOrders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se existe uma ordem de fabrico com o ID indicado
    public async Task<bool> Exists(int id) =>
        await _context.ManufacturingOrders
            .AnyAsync(mo => mo.Id == id);

    // Devolve todas as ordens de fabrico associadas
    // à mesma encomenda de cliente
    //
    // Uma ClientOrder pode dar origem a várias ManufacturingOrders.
    // Este método permite verificar se todas já foram concluídas,
    // para enviar a notificação final ao cliente.
    public async Task<IEnumerable<ManufacturingOrderModel>>
        GetByClientOrderId(int clientOrderId) =>
        await _context.ManufacturingOrders

            // Filtra pela encomenda de cliente
            .Where(mo =>
                mo.ClientOrderId == clientOrderId
            )

            // Executa a consulta e devolve uma lista
            .ToListAsync();
}