using ApiTexPact.DTO;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;

namespace ApiTexPact.Services;

public class WipDashboardService : IWipDashboardService
{
    private readonly IWipDashboardRepository _repository;

    public WipDashboardService(IWipDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<WipDashboardResultDTO> GetWipDashboardAsync()
    {
        var inProgressItems = await _repository.GetInProgressAsync();
        var waitingItems = await _repository.GetWaitingAsync();
        var completed = await _repository.GetCompletedCountAsync();
        var graph = BuildGraph(waitingItems, inProgressItems);

        return new WipDashboardResultDTO
        {
            TotalProducts = waitingItems.Select(i => i.ProductId)
                .Concat(inProgressItems.Select(i => i.ProductId))
                .Distinct()
                .Count(),
            Waiting = waitingItems.Count,
            InProgress = inProgressItems.Count,
            Completed = completed,
            ActiveLines = inProgressItems.Select(i => i.ProductionLineId).Distinct().Count(),
            WaitingItems = waitingItems,
            Items = inProgressItems,
            Graph = graph
        };
    }

    private static WipGraphDTO BuildGraph(List<WaitingItemDTO> waitingItems, List<WipItemDTO> inProgressItems)
    {
        var nodes = new List<WipGraphNodeDTO>();
        var edges = new List<WipGraphEdgeDTO>();
        var nodeIds = new HashSet<string>();

        void AddNode(string id, string label, string type, string? subtitle = null)
        {
            if (nodeIds.Add(id))
                nodes.Add(new WipGraphNodeDTO { Id = id, Label = label, Type = type, Subtitle = subtitle });
        }

        void AddEdge(string source, string target, string type)
        {
            edges.Add(new WipGraphEdgeDTO { Source = source, Target = target, Type = type });
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

        return new WipGraphDTO { Nodes = nodes, Edges = edges };
    }
}