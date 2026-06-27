using Drivolution.Data;
using Drivolution.DTO.Analytics;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly ApplicationDbContext _context;

    public AnalyticsRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PhaseDurationDTO>> GetPhaseDurationsAsync()
    {
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

    public async Task<List<ReworkRateDTO>> GetReworkRatesAsync()
    {
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

    public async Task<List<ThroughputDTO>> GetThroughputAsync()
    {
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