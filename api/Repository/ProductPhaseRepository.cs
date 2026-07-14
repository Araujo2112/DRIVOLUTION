using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Implementa o contrato definido em IProductPhaseRepository
public class ProductPhaseRepository : IProductPhaseRepository
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o contexto da base de dados
    public ProductPhaseRepository(ApplicationDbContext context)
        => _context = context;

    // Devolve todas as fases de um produto
    public async Task<IEnumerable<ProductPhaseModel>> GetByProduct(int productId) =>
        await _context.ProductPhases

            // Filtra apenas as fases do produto indicado
            .Where(pp => pp.ProductId == productId)

            // Inclui os dados da fase de fabrico relacionada
            .Include(pp => pp.ManufacturingPhase)

            // Inclui os dados da workstation relacionada
            .Include(pp => pp.Workstation)

            // Ordena as fases da mais antiga para a mais recente
            .OrderBy(pp => pp.DatetimeIni)

            // Executa a consulta e devolve uma lista
            .ToListAsync();

    // Devolve a fase atual de um produto
    public async Task<ProductPhaseModel?> GetCurrentByProduct(int productId) =>
        await _context.ProductPhases

            // Procura fases do produto que ainda não terminaram
            .Where(pp =>
                pp.ProductId == productId &&
                pp.DatetimeEnd == null
            )

            // Inclui a informação da fase de fabrico
            .Include(pp => pp.ManufacturingPhase)

            // Inclui a workstation
            .Include(pp => pp.Workstation)

            // Ordena da fase mais recente para a mais antiga
            .OrderByDescending(pp => pp.DatetimeIni)

            // Devolve a primeira fase encontrada ou null
            .FirstOrDefaultAsync();

    // Devolve as fases atuais de vários produtos
    public async Task<List<ProductPhaseModel>> GetCurrentByProducts(
        List<int> productIds) =>
        await _context.ProductPhases

            // Procura fases abertas cujos produtos estejam na lista recebida
            .Where(pp =>
                productIds.Contains(pp.ProductId) &&
                pp.DatetimeEnd == null
            )

            // Inclui a informação da fase de fabrico
            .Include(pp => pp.ManufacturingPhase)

            // Inclui a workstation
            .Include(pp => pp.Workstation)

            // Executa a consulta
            .ToListAsync();

    // Devolve todas as fases abertas de um produto
    public async Task<IEnumerable<ProductPhaseModel>> GetAllOpenByProduct(
        int productId) =>
        await _context.ProductPhases

            // Uma fase está aberta quando DatetimeEnd é null
            .Where(pp =>
                pp.ProductId == productId &&
                pp.DatetimeEnd == null
            )

            // Ordena pela data de início
            .OrderBy(pp => pp.DatetimeIni)

            .ToListAsync();

    // Procura uma fase pelo ID
    public async Task<ProductPhaseModel?> GetById(int id) =>
        await _context.ProductPhases.FindAsync(id);

    // Cria uma nova fase de produto
    public async Task<ProductPhaseModel> Create(
        ProductPhaseModel entity)
    {
        // Garante que a data de início é tratada como UTC
        entity.DatetimeIni = DateTime.SpecifyKind(
            entity.DatetimeIni,
            DateTimeKind.Utc
        );

        // Adiciona a nova fase ao contexto
        _context.ProductPhases.Add(entity);

        // Guarda a alteração na base de dados
        await _context.SaveChangesAsync();

        // Devolve a entidade criada
        return entity;
    }

    // Atualiza uma fase existente
    public async Task Update(ProductPhaseModel entity)
    {
        // Garante que a data de início é UTC
        entity.DatetimeIni = DateTime.SpecifyKind(
            entity.DatetimeIni,
            DateTimeKind.Utc
        );

        // Se existir data de fim, também garante que é UTC
        if (entity.DatetimeEnd.HasValue)
        {
            entity.DatetimeEnd = DateTime.SpecifyKind(
                entity.DatetimeEnd.Value,
                DateTimeKind.Utc
            );
        }

        // Marca a entidade como atualizada
        _context.ProductPhases.Update(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();
    }

    // Devolve todas as fases abertas com informação
    // sobre a fase de fabrico e o produto
    public async Task<IEnumerable<ProductPhaseModel>>
        GetOpenPhasesWithPhaseInfoAsync() =>
        await _context.ProductPhases

            // Procura apenas fases ainda abertas
            .Where(pp => pp.DatetimeEnd == null)

            // Inclui os dados da fase de fabrico
            .Include(pp => pp.ManufacturingPhase)

            // Inclui os dados do produto
            .Include(pp => pp.Product)

            .ToListAsync();

    // Procura uma fase pelo ID e inclui a fase de fabrico
    public async Task<ProductPhaseModel?> GetByIdAsync(int id) =>
        await _context.ProductPhases

            // Carrega também os dados de ManufacturingPhase
            .Include(pp => pp.ManufacturingPhase)

            // Procura a primeira fase com o ID indicado
            .FirstOrDefaultAsync(pp => pp.Id == id);

    // Devolve a fase aberta mais recente de uma linha de produção
    public async Task<ProductPhaseModel?>
        GetCurrentOpenByProductionLine(int productionLineId) =>
        await _context.ProductPhases

            // Procura fases abertas na linha indicada
            .Where(pp =>
                pp.DatetimeEnd == null &&
                pp.Workstation.ProductionLineId == productionLineId
            )

            // Inclui os dados relacionados
            .Include(pp => pp.ManufacturingPhase)
            .Include(pp => pp.Workstation)
            .Include(pp => pp.Product)

            // Ordena da fase mais recente para a mais antiga
            .OrderByDescending(pp => pp.DatetimeIni)

            // Devolve apenas a mais recente ou null
            .FirstOrDefaultAsync();

    // Devolve todas as fases abertas de uma linha de produção
    public async Task<IEnumerable<ProductPhaseModel>>
        GetAllOpenByProductionLine(int productionLineId) =>
        await _context.ProductPhases

            // Filtra pelas fases abertas da linha indicada
            .Where(pp =>
                pp.DatetimeEnd == null &&
                pp.Workstation.ProductionLineId == productionLineId
            )

            // Inclui os dados relacionados
            .Include(pp => pp.ManufacturingPhase)
            .Include(pp => pp.Workstation)
            .Include(pp => pp.Product)

            // Ordena da fase mais antiga para a mais recente
            .OrderBy(pp => pp.DatetimeIni)

            .ToListAsync();
}