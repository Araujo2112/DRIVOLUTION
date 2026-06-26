using Drivolution.Data;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/production-lines")]
public class ProductionLineStatusController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IEtaPredictionService _etaService;

    public ProductionLineStatusController(ApplicationDbContext context, IEtaPredictionService etaService)
    {
        _context = context;
        _etaService = etaService;
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
                    NULL::timestamp AS "EstimatedFinish",
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

        // EstimatedFinish passa a ser o ETA completo do carro (todas as fases
        // que faltam até saír da linha), não só o tempo desta fase — uma linha
        // representa o percurso todo (todas as workstations em sequência),
        // não uma estação isolada.
        foreach (var row in status)
        {
            if (row.ProductId == null) continue;

            var eta = await _etaService.PredictForProduct(row.ProductId.Value);
            if (eta != null)
                row.EstimatedFinish = eta.EstimatedCompletion;
        }

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