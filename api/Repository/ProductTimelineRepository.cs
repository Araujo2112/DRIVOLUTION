using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class ProductTimelineRepository : IProductTimelineRepository
{
    private readonly ApplicationDbContext _context;

    public ProductTimelineRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> ProductExists(int productId) =>
        await _context.Database
            .SqlQueryRaw<int>("SELECT id FROM product WHERE id = {0}", productId)
            .AnyAsync();

    public async Task<List<ProductTimelineDTO>> GetTimeline(int productId) =>
        await _context.Database
            .SqlQueryRaw<ProductTimelineDTO>(
                """
                SELECT
                    pp.id                 AS "ProductPhaseId",
                    p.id                  AS "ProductId",
                    p.model_id            AS "ModelId",
                    p.serial_number       AS "SerialNumber",
                    mp.id                 AS "ManufacturingPhaseId",
                    mp.name               AS "PhaseName",
                    w.type                AS "Workstation",
                    pp.datetime_ini       AS "StartedAt",
                    pp.datetime_end       AS "EndedAt",
                    CASE
                        WHEN pp.datetime_end IS NULL THEN NULL
                        ELSE EXTRACT(EPOCH FROM (pp.datetime_end - pp.datetime_ini))::INT
                    END                   AS "DurationSeconds",
                    pp.result             AS "Result",
                    pp.notes              AS "Notes",
                    NULL::timestamp       AS "EstimatedFinish"
                FROM product_phase pp
                JOIN product p ON p.id = pp.product_id
                JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id
                JOIN workstation w ON w.id = pp.workstation_id
                WHERE pp.product_id = {0}
                ORDER BY pp.datetime_ini
                """,
                productId
            )
            .ToListAsync();
}