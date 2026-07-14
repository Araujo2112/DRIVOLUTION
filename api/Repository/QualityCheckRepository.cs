using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Implementa o contrato definido em IQualityCheckRepository
public class QualityCheckRepository : IQualityCheckRepository
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o contexto da base de dados
    public QualityCheckRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Devolve todos os controlos de qualidade de um produto
    public async Task<IEnumerable<QualityCheckModel>> GetByProduct(int productId) =>
        await _context.QualityChecks

            // Filtra apenas os controlos do produto indicado
            .Where(qc => qc.ProductId == productId)

            // Inclui também os dados da fase de fabrico relacionada
            .Include(qc => qc.ManufacturingPhase)

            // Executa a consulta e devolve os resultados numa lista
            .ToListAsync();

    // Procura um controlo de qualidade pelo seu ID
    public async Task<QualityCheckModel?> GetById(int id) =>
        await _context.QualityChecks.FindAsync(id);

    // Cria um novo controlo de qualidade
    public async Task<QualityCheckModel> Create(QualityCheckModel entity)
    {
        // Adiciona a entidade ao contexto
        _context.QualityChecks.Add(entity);

        // Guarda a nova entidade na base de dados
        await _context.SaveChangesAsync();

        // Devolve o controlo de qualidade criado
        return entity;
    }

    // Atualiza um controlo de qualidade existente
    public async Task Update(QualityCheckModel entity)
    {
        // Marca a entidade como atualizada
        _context.QualityChecks.Update(entity);

        // Guarda as alterações na base de dados
        await _context.SaveChangesAsync();
    }

    // Verifica se um produto passou no controlo de qualidade
    // de uma fase de fabrico específica
    public async Task<bool> HasPassedForProductPhaseAsync(
        int productId,
        int manufacturingPhaseId) =>
        await _context.QualityChecks.AnyAsync(qc =>

            // O controlo tem de pertencer ao produto indicado
            qc.ProductId == productId &&

            // Tem de pertencer à fase indicada
            qc.ManufacturingPhaseId == manufacturingPhaseId &&

            // E o estado tem de ser "passed"
            qc.Status == QualityStatus.Passed
        );
}