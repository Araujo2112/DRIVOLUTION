using Drivolution.Data;
using Drivolution.DTO.Analytics;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por obter métricas e indicadores analíticos da produção
public class AnalyticsRepository : IAnalyticsRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Calcula a duração média de cada fase de fabrico
    public async Task<List<PhaseDurationDTO>> GetPhaseDurationsAsync()
    {
        // Executa diretamente uma query SQL para calcular
        // a duração média (em minutos) e o número de execuções
        return await _context.Database
            .SqlQueryRaw<PhaseDurationDTO>(
                """
                SELECT
                    mp.id AS "ManufacturingPhaseId",
                    mp.name AS "PhaseName",
                    ROUND(AVG(EXTRACT(EPOCH FROM (pp.datetime_end - pp.datetime_ini)) / 60.0), 2)::double precision AS "AverageDurationMinutes",
                    COUNT(*)::int AS "CompletedCount"
                FROM product_phase pp
                JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id
                WHERE pp.datetime_end IS NOT NULL
                GROUP BY mp.id, mp.name
                ORDER BY mp.id
                """
            )
            .ToListAsync();
    }

    // Calcula a taxa de retrabalho (rework) por fase de fabrico
    public async Task<List<ReworkRateDTO>> GetReworkRatesAsync()
    {
        // Conta o número total de Quality Checks e quantos falharam,
        // calculando depois a respetiva percentagem
        return await _context.Database
            .SqlQueryRaw<ReworkRateDTO>(
                """
                SELECT
                    mp.id AS "ManufacturingPhaseId",
                    mp.name AS "PhaseName",
                    COUNT(qc.id)::int AS "TotalChecks",
                    SUM(
                        CASE
                            WHEN LOWER(COALESCE(qc.status, '')) IN ('failed', 'reproved', 'reprovado', 'fail')
                            THEN 1
                            ELSE 0
                        END
                    )::int AS "FailedChecks",
                    CASE
                        WHEN COUNT(qc.id) = 0 THEN 0
                        ELSE ROUND(
                            (
                                SUM(
                                    CASE
                                        WHEN LOWER(COALESCE(qc.status, '')) IN ('failed', 'reproved', 'reprovado', 'fail')
                                        THEN 1
                                        ELSE 0
                                    END
                                )::numeric / COUNT(qc.id)::numeric
                            ) * 100,
                            2
                        )::double precision
                    END AS "ReworkRatePercent"
                FROM manufacturing_phase mp
                LEFT JOIN quality_check qc ON qc.manufacturing_phase_id = mp.id
                GROUP BY mp.id, mp.name
                ORDER BY mp.id
                """
            )
            .ToListAsync();
    }

    // Calcula o throughput da produção ao longo do tempo
    public async Task<List<ThroughputDTO>> GetThroughputAsync()
    {
        // Considera apenas produtos que terminaram todas as fases
        // e agrupa-os por dia de conclusão
        return await _context.Database
            .SqlQueryRaw<ThroughputDTO>(
                """
                WITH completed_products AS (
                    SELECT
                        p.id AS product_id,
                        DATE_TRUNC('day', MAX(pp.datetime_end))::timestamp AS completed_day
                    FROM product p
                    JOIN product_phase pp ON pp.product_id = p.id
                    GROUP BY p.id
                    HAVING COUNT(*) = COUNT(pp.datetime_end)
                )
                SELECT
                    completed_day AS "Period",
                    COUNT(product_id)::int AS "CompletedProducts"
                FROM completed_products
                GROUP BY completed_day
                ORDER BY completed_day
                """
            )
            .ToListAsync();
    }
}