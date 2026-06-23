using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class WipDashboardRepository : IWipDashboardRepository
{
    private readonly ApplicationDbContext _context;

    public WipDashboardRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WipItemDTO>> GetInProgressAsync()
    {
        return await _context.Database
            .SqlQueryRaw<WipItemDTO>(
                """
                SELECT
                    p.id AS "ProductId",
                    p.serial_number AS "SerialNumber",
                    pl.id AS "ProductionLineId",
                    pl.name AS "ProductionLineName",
                    w.id AS "WorkstationId",
                    w.type AS "Workstation",
                    mp.name AS "CurrentPhase",
                    mp.estimated_duration AS "EstimatedDuration",
                    mp.time_threshold_pct AS "TimeThresholdPct",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",
                    pp.result AS "Result",
                    'in_progress' AS "WipStatus",
                    EXTRACT(EPOCH FROM (NOW() - pp.datetime_ini))::INT AS "ElapsedSeconds"
                FROM product_phase pp
                JOIN product p ON p.id = pp.product_id
                JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id
                JOIN workstation w ON w.id = pp.workstation_id
                JOIN production_line pl ON pl.id = w.production_line_id
                WHERE pp.datetime_end IS NULL
                ORDER BY pp.datetime_ini DESC
                """
            )
            .ToListAsync();
    }

    public async Task<List<WaitingItemDTO>> GetWaitingAsync()
    {
        return await _context.Database
            .SqlQueryRaw<WaitingItemDTO>(
                """
                WITH active_phase AS (
                    SELECT DISTINCT product_id
                    FROM product_phase
                    WHERE datetime_end IS NULL
                ),
                last_phase AS (
                    SELECT DISTINCT ON (pp.product_id)
                        pp.product_id,
                        pp.manufacturing_phase_id,
                        mp.name AS phase_name,
                        pp.datetime_end
                    FROM product_phase pp
                    JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id
                    WHERE pp.datetime_end IS NOT NULL
                    ORDER BY pp.product_id, pp.datetime_end DESC
                ),
                current_support AS (
                    SELECT DISTINCT ON (sp.product_id)
                        sp.product_id,
                        sp.support_id,
                        s.rfid_tag,
                        s.type AS support_type
                    FROM supported_product sp
                    JOIN support s ON s.id = sp.support_id
                    WHERE sp.datetime_end IS NULL
                    ORDER BY sp.product_id, sp.datetime_ini DESC
                ),
                current_location AS (
                    SELECT DISTINCT ON (lh.support_id)
                        lh.support_id,
                        lh.workstation_id,
                        w.type AS workstation,
                        pl.id AS production_line_id,
                        pl.name AS production_line_name
                    FROM localization_history lh
                    JOIN workstation w ON w.id = lh.workstation_id
                    JOIN production_line pl ON pl.id = w.production_line_id
                    WHERE lh.datetime_end IS NULL
                    ORDER BY lh.support_id, lh.datetime_ini DESC
                )
                SELECT
                    p.id AS "ProductId",
                    p.serial_number AS "SerialNumber",
                    cm.name AS "ModelName",
                    mo.id AS "ManufacturingOrderId",
                    mo.manufacturing_order_number AS "ManufacturingOrderNumber",
                    mo.status AS "ManufacturingOrderStatus",
                    cs.support_id AS "SupportId",
                    cs.rfid_tag AS "RfidTag",
                    cs.support_type AS "SupportType",
                    cl.production_line_id AS "ProductionLineId",
                    cl.production_line_name AS "ProductionLineName",
                    cl.workstation_id AS "WorkstationId",
                    cl.workstation AS "Workstation",
                    lp.phase_name AS "LastPhase",
                    lp.datetime_end AS "LastPhaseEndedAt",
                    next_mp.name AS "NextPhase",
                    CASE
                        WHEN cs.support_id IS NULL THEN 'waiting_support'
                        WHEN cl.workstation_id IS NULL THEN 'waiting_line'
                        ELSE 'waiting_next_phase'
                    END AS "QueueReason"
                FROM product p
                JOIN manufacturing_order mo ON mo.id = p.manufacturing_order_id
                JOIN model cm ON cm.id = p.model_id
                LEFT JOIN active_phase ap ON ap.product_id = p.id
                LEFT JOIN last_phase lp ON lp.product_id = p.id
                LEFT JOIN phase_sequence last_ps
                    ON last_ps.model_id = p.model_id
                   AND last_ps.manufacturing_phase_id = lp.manufacturing_phase_id
                LEFT JOIN phase_sequence next_ps
                    ON next_ps.model_id = p.model_id
                   AND next_ps."order" = COALESCE(last_ps."order", 0) + 1
                LEFT JOIN manufacturing_phase next_mp ON next_mp.id = next_ps.manufacturing_phase_id
                LEFT JOIN current_support cs ON cs.product_id = p.id
                LEFT JOIN current_location cl ON cl.support_id = cs.support_id
                WHERE ap.product_id IS NULL
                  AND COALESCE(mo.status, '') <> 'completed'
                ORDER BY
                    CASE
                        WHEN cs.support_id IS NULL THEN 1
                        WHEN cl.workstation_id IS NULL THEN 2
                        ELSE 3
                    END,
                    mo.start_date,
                    p.id
                """
            )
            .ToListAsync();
    }

    public async Task<int> GetCompletedCountAsync()
    {
        var result = await _context.Database
            .SqlQueryRaw<CountDTO>(
                """
                SELECT COUNT(DISTINCT p.id)::INT AS "Value"
                FROM product p
                JOIN manufacturing_order mo ON mo.id = p.manufacturing_order_id
                WHERE mo.status = 'completed'
                """
            )
            .FirstAsync();

        return result.Value;
    }
}