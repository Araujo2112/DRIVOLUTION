using ApiTexPact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/products")]
public class ProductPerformanceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductPerformanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{productId}/performance")]
    public async Task<IActionResult> GetProductPerformance(int productId)
    {
        var productExists = await _context.Database
            .SqlQueryRaw<int>("SELECT id FROM product WHERE id = {0}", productId)
            .AnyAsync();

        if (!productExists)
            return NotFound("Product does not exist.");

        var phases = await _context.Database
            .SqlQueryRaw<ProductPhasePerformanceDto>(
                """
                SELECT
                    pp.id AS "ProductPhaseId",
                    pp.product_id AS "ProductId",
                    pp.manufacturing_phase_id AS "ManufacturingPhaseId",
                    mp.name AS "PhaseName",
                    mp.estimated_duration AS "EstimatedDuration",
                    pp.workstation_id AS "WorkstationId",
                    w.type AS "Workstation",
                    pp.datetime_ini AS "StartedAt",
                    pp.datetime_end AS "EndedAt",
                    CASE
                        WHEN pp.datetime_end IS NULL THEN NULL
                        ELSE EXTRACT(EPOCH FROM (pp.datetime_end - pp.datetime_ini))::INT
                    END AS "RealDurationSeconds",
                    pp.result AS "Result"
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

        if (!phases.Any())
            return BadRequest("Product has no production history.");

        var completedPhases = phases
            .Where(p => p.RealDurationSeconds.HasValue)
            .ToList();

        var totalRealDurationSeconds = completedPhases.Sum(p => p.RealDurationSeconds ?? 0);
        var totalEstimatedDuration = phases.Sum(p => p.EstimatedDuration);

        var bottleneck = completedPhases
            .OrderByDescending(p => p.RealDurationSeconds)
            .FirstOrDefault();

        return Ok(new
        {
            productId,
            status = phases.Any(p => p.EndedAt == null) ? "in_progress" : "completed",
            totalEstimatedDuration,
            totalRealDurationSeconds,
            delaySeconds = totalRealDurationSeconds - totalEstimatedDuration,
            bottleneck = bottleneck == null ? null : new
            {
                bottleneck.PhaseName,
                bottleneck.Workstation,
                durationSeconds = bottleneck.RealDurationSeconds
            },
            phases
        });
    }

    public class ProductPhasePerformanceDto
    {
        public int ProductPhaseId { get; set; }
        public int ProductId { get; set; }
        public int ManufacturingPhaseId { get; set; }
        public string? PhaseName { get; set; }
        public int EstimatedDuration { get; set; }
        public int WorkstationId { get; set; }
        public string? Workstation { get; set; }
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int? RealDurationSeconds { get; set; }
        public string? Result { get; set; }
    }
}