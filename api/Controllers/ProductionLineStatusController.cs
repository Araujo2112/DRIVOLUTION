using Drivolution.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/production-lines")]
public class ProductionLineStatusController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductionLineStatusController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("status")]
    public async Task<IActionResult> GetProductionLineStatus()
    {
        var status = await _context.Database
            .SqlQueryRaw<ProductionLineStatusDto>(
                """
                SELECT
                    pl.id AS "ProductionLineId",
                    pl.name AS "ProductionLineName",
                    pl.location AS "Location",
                    pl.status AS "LineStatus",
                    w.id AS "WorkstationId",
                    w.type AS "WorkstationType",
                    p.id AS "ProductId",
                    p.serial_number AS "SerialNumber",
                    mp.name AS "CurrentPhase",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_ini + (mp.estimated_duration || ' seconds')::interval AS "EstimatedFinish",
                    pp.result AS "ProductStatus"
                FROM workstation w
                JOIN production_line pl ON pl.id = w.production_line_id
                LEFT JOIN product_phase pp 
                    ON pp.workstation_id = w.id 
                   AND pp.datetime_end IS NULL
                LEFT JOIN product p ON p.id = pp.product_id
                LEFT JOIN manufacturing_phase mp ON mp.id = pp.manufacturing_phase_id
                ORDER BY pl.id, w.id
                """
            )
            .ToListAsync();

        return Ok(status);
    }

    public class ProductionLineStatusDto
    {
        public int ProductionLineId { get; set; }
        public string? ProductionLineName { get; set; }
        public string? Location { get; set; }
        public string? LineStatus { get; set; }
        public int WorkstationId { get; set; }
        public string? WorkstationType { get; set; }
        public int? ProductId { get; set; }
        public string? SerialNumber { get; set; }
        public string? CurrentPhase { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? EstimatedFinish { get; set; }
        public string? ProductStatus { get; set; }
    }
}