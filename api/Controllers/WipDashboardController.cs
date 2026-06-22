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
        var inProgressItems = await _context.Database
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

        var waitingItems = await _context.Database
            .SqlQueryRaw<WaitingItemDto>(
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

        var completedProducts = await _context.Database
            .SqlQueryRaw<CountDto>(
                """
                SELECT COUNT(DISTINCT p.id)::INT AS "Value"
                FROM product p
                JOIN manufacturing_order mo ON mo.id = p.manufacturing_order_id
                WHERE mo.status = 'completed'
                """
            )
            .FirstAsync();

        var graph = BuildGraph(waitingItems, inProgressItems);

        return Ok(new
        {
            totalProducts = waitingItems.Select(i => i.ProductId).Concat(inProgressItems.Select(i => i.ProductId)).Distinct().Count(),
            waiting = waitingItems.Count,
            inProgress = inProgressItems.Count,
            completed = completedProducts.Value,
            activeLines = inProgressItems.Select(i => i.ProductionLineId).Distinct().Count(),
            waitingItems,
            items = inProgressItems,
            graph
        });
    }

    private static object BuildGraph(List<WaitingItemDto> waitingItems, List<WipItemDto> inProgressItems)
    {
        var nodes = new List<GraphNodeDto>();
        var edges = new List<GraphEdgeDto>();
        var nodeIds = new HashSet<string>();

        void AddNode(string id, string label, string type, string? subtitle = null)
        {
            if (nodeIds.Add(id))
            {
                nodes.Add(new GraphNodeDto
                {
                    Id = id,
                    Label = label,
                    Type = type,
                    Subtitle = subtitle
                });
            }
        }

        void AddEdge(string source, string target, string type)
        {
            edges.Add(new GraphEdgeDto
            {
                Source = source,
                Target = target,
                Type = type
            });
        }

        AddNode("factory", "Produção", "factory", "Estado global");

        foreach (var item in waitingItems)
        {
            var productNode = $"product-{item.ProductId}";
            AddNode(productNode, item.SerialNumber ?? $"Produto {item.ProductId}", "waitingProduct", item.QueueReason);
            AddEdge("factory", productNode, "waiting");

            if (!string.IsNullOrWhiteSpace(item.NextPhase))
            {
                var phaseNode = $"phase-waiting-{item.ProductId}";
                AddNode(phaseNode, item.NextPhase, "phase", "próxima fase");
                AddEdge(productNode, phaseNode, "next");
            }

            if (item.SupportId != null)
            {
                var skidNode = $"support-{item.SupportId}";
                AddNode(skidNode, item.RfidTag ?? $"Skid {item.SupportId}", "support", item.SupportType);
                AddEdge(skidNode, productNode, "carries");
            }
        }

        foreach (var item in inProgressItems)
        {
            var lineNode = $"line-{item.ProductionLineId}";
            var wsNode = $"ws-{item.WorkstationId}";
            var productNode = $"product-{item.ProductId}";
            var phaseNode = $"phase-active-{item.ProductId}";

            AddNode(lineNode, item.ProductionLineName ?? $"Linha {item.ProductionLineId}", "line");
            AddNode(wsNode, item.Workstation ?? $"WS {item.WorkstationId}", "workstation");
            AddNode(productNode, item.SerialNumber ?? $"Produto {item.ProductId}", "activeProduct", item.WipStatus);
            AddNode(phaseNode, item.CurrentPhase ?? "Fase atual", "phase", "em curso");

            AddEdge("factory", lineNode, "contains");
            AddEdge(lineNode, wsNode, "contains");
            AddEdge(wsNode, productNode, "working");
            AddEdge(productNode, phaseNode, "currentPhase");
        }

        return new
        {
            nodes,
            edges
        };
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

    public class WaitingItemDto
    {
        public int ProductId { get; set; }
        public string? SerialNumber { get; set; }
        public string? ModelName { get; set; }
        public int ManufacturingOrderId { get; set; }
        public string? ManufacturingOrderNumber { get; set; }
        public string? ManufacturingOrderStatus { get; set; }
        public int? SupportId { get; set; }
        public string? RfidTag { get; set; }
        public string? SupportType { get; set; }
        public int? ProductionLineId { get; set; }
        public string? ProductionLineName { get; set; }
        public int? WorkstationId { get; set; }
        public string? Workstation { get; set; }
        public string? LastPhase { get; set; }
        public DateTime? LastPhaseEndedAt { get; set; }
        public string? NextPhase { get; set; }
        public string? QueueReason { get; set; }
    }

    public class CountDto
    {
        public int Value { get; set; }
    }

    public class GraphNodeDto
    {
        public string Id { get; set; } = "";
        public string Label { get; set; } = "";
        public string Type { get; set; } = "";
        public string? Subtitle { get; set; }
    }

    public class GraphEdgeDto
    {
        public string Source { get; set; } = "";
        public string Target { get; set; } = "";
        public string Type { get; set; } = "";
    }
}
