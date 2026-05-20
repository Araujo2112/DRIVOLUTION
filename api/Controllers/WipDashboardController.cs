using ApiTexPact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/production-lines")]
public class WipDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WipDashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("wip")]
    public async Task<IActionResult> GetWipDashboard()
    {
        var items = await _context.Database
            .SqlQueryRaw<WipItemDto>(
                """
                SELECT
                    p.id AS "ProductId",
                    p.serial_number AS "SerialNumber",
                    pl.id AS "ProductionLineId",
                    pl.name AS "ProductionLineName",
                    w.id AS "WorkstationId",
                    w.type AS "Workstation",
                    mp.name AS "CurrentPhase",
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

        return Ok(new
        {
            totalProducts = items.Select(i => i.ProductId).Distinct().Count(),
            inProgress = items.Count,
            completed = 0,
            items
        });
    }

    public class WipItemDto
    {
        public int ProductId { get; set; }
        public string? SerialNumber { get; set; }
        public int ProductionLineId { get; set; }
        public string? ProductionLineName { get; set; }
        public int WorkstationId { get; set; }
        public string? Workstation { get; set; }
        public string? CurrentPhase { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? Result { get; set; }
        public string? WipStatus { get; set; }
        public int ElapsedSeconds { get; set; }
    }
}