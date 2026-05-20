using ApiTexPact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/products")]
public class ProductTimelineController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductTimelineController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{productId}/timeline")]
    public async Task<IActionResult> GetProductTimeline(int productId)
    {
        var productExists = await _context.Database
            .SqlQueryRaw<int>("SELECT id FROM product WHERE id = {0}", productId)
            .AnyAsync();

        if (!productExists)
            return NotFound("Product does not exist.");

        var timeline = await _context.Database
            .SqlQueryRaw<ProductTimelineDto>(
                """
                SELECT
                    pp.id AS "ProductPhaseId",
                    p.id AS "ProductId",
                    p.serial_number AS "SerialNumber",
                    mp.name AS "PhaseName",
                    w.type AS "Workstation",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",
                    CASE
                        WHEN pp.datetime_end IS NULL THEN NULL
                        ELSE EXTRACT(EPOCH FROM (pp.datetime_end - pp.datetime_ini))::INT
                    END AS "DurationSeconds",
                    pp.result AS "Result",
                    pp.condition AS "Condition",
                    pp.notes AS "Notes"
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

        if (!timeline.Any())
            return BadRequest("Product has no timeline yet.");

        return Ok(new
        {
            productId,
            serialNumber = timeline.First().SerialNumber,
            status = timeline.Any(t => t.EndedAt == null) ? "in_progress" : "completed",
            phases = timeline
        });
    }

    public class ProductTimelineDto
    {
        public int ProductPhaseId { get; set; }
        public int ProductId { get; set; }
        public string? SerialNumber { get; set; }
        public string? PhaseName { get; set; }
        public string? Workstation { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? DurationSeconds { get; set; }
        public string? Result { get; set; }
        public string? Condition { get; set; }
        public string? Notes { get; set; }
    }
}