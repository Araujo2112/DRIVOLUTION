using ApiTexPact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/products")]
public class ProductHistoryController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{productId}/history")]
    public async Task<IActionResult> GetProductHistory(int productId)
    {
        var productExists = await _context.Database
            .SqlQueryRaw<int>("SELECT id FROM product WHERE id = {0}", productId)
            .AnyAsync();

        if (!productExists)
            return NotFound("Product does not exist.");

        var history = await _context.Database
            .SqlQueryRaw<ProductPhaseHistoryDto>(
                """
                SELECT 
                    pp.id AS "ProductPhaseId",
                    pp.product_id AS "ProductId",
                    pp.manufacturing_phase_id AS "ManufacturingPhaseId",
                    mp.name AS "PhaseName",
                    pp.workstation_id AS "WorkstationId",
                    w.type AS "Workstation",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",
                    pp.result AS "Result",
                    pp.condition AS "Condition",
                    pp.notes AS "Notes"
                FROM product_phase pp
                JOIN manufacturing_phase mp 
                    ON mp.id = pp.manufacturing_phase_id
                JOIN workstation w 
                    ON w.id = pp.workstation_id
                WHERE pp.product_id = {0}
                ORDER BY pp.datetime_ini
                """,
                productId
            )
            .ToListAsync();

        return Ok(history);
    }

    [HttpGet("{productId}/current")]
    public async Task<IActionResult> GetCurrentProductPhase(int productId)
    {
        var current = await _context.Database
            .SqlQueryRaw<ProductPhaseHistoryDto>(
                """
                SELECT 
                    pp.id AS "ProductPhaseId",
                    pp.product_id AS "ProductId",
                    pp.manufacturing_phase_id AS "ManufacturingPhaseId",
                    mp.name AS "PhaseName",
                    pp.workstation_id AS "WorkstationId",
                    w.type AS "Workstation",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",
                    pp.result AS "Result",
                    pp.condition AS "Condition",
                    pp.notes AS "Notes"
                FROM product_phase pp
                JOIN manufacturing_phase mp 
                    ON mp.id = pp.manufacturing_phase_id
                JOIN workstation w 
                    ON w.id = pp.workstation_id
                WHERE pp.product_id = {0}
                  AND pp.datetime_end IS NULL
                ORDER BY pp.datetime_ini DESC
                LIMIT 1
                """,
                productId
            )
            .FirstOrDefaultAsync();

        if (current == null)
            return NotFound("Product has no active phase.");

        return Ok(current);
    }

    public class ProductPhaseHistoryDto
    {
        public int ProductPhaseId { get; set; }
        public int ProductId { get; set; }
        public int ManufacturingPhaseId { get; set; }
        public string? PhaseName { get; set; }
        public int WorkstationId { get; set; }
        public string? Workstation { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public string? Result { get; set; }
        public string? Condition { get; set; }
        public string? Notes { get; set; }
    }
}