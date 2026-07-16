using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável por construir o dashboard WIP (Work In Progress)
public class WipDashboardService : IWipDashboardService
{
    // Repository que fornece os dados do dashboard
    private readonly IWipDashboardRepository _repository;

    // Service responsável pelas previsões de duração das fases
    private readonly IEtaPredictionService _etaService;

    // O ASP.NET injeta automaticamente as dependências
    public WipDashboardService(
        IWipDashboardRepository repository,
        IEtaPredictionService etaService)
    {
        _repository = repository;
        _etaService = etaService;
    }

    // Obtém toda a informação necessária para o dashboard
    public async Task<WipDashboardResultDTO> GetWipDashboardAsync()
    {
        // Produtos atualmente em produção
        var inProgressItems = await _repository.GetInProgressAsync();

        // Produtos em espera
        var waitingItems = await _repository.GetWaitingAsync();

        // Número de produtos concluídos
        var completed = await _repository.GetCompletedCountAsync();

        // Calcula previsões de duração para as fases em curso
        var predictions = await _etaService.PredictCurrentPhaseDurationsForWip(inProgressItems);

        // Associa a previsão a cada produto
        foreach (var item in inProgressItems)
        {
            if (predictions.TryGetValue(item.ProductId, out var prediction))
            {
                item.PredictedPhaseDurationSeconds = prediction.Seconds;
                item.PredictedPhaseDurationIsMl = prediction.IsMlPrediction;
            }
        }

        // Constrói o grafo apresentado no dashboard
        var graph = BuildGraph(waitingItems, inProgressItems);

        // Devolve toda a informação agregada
        return new WipDashboardResultDTO
        {
            TotalProducts = waitingItems
                .Select(i => i.ProductId)
                .Concat(inProgressItems.Select(i => i.ProductId))
                .Distinct()
                .Count(),

            Waiting = waitingItems.Count,
            InProgress = inProgressItems.Count,
            Completed = completed,

            // Número de linhas de produção atualmente ativas
            ActiveLines = inProgressItems
                .Select(i => i.ProductionLineId)
                .Distinct()
                .Count(),

            WaitingItems = waitingItems,
            Items = inProgressItems,
            Graph = graph
        };
    }

    // Constrói o grafo apresentado na interface
    private static WipGraphDTO BuildGraph(
        List<WaitingItemDTO> waitingItems,
        List<WipItemDTO> inProgressItems)
    {
        var nodes = new List<WipGraphNodeDTO>();
        var edges = new List<WipGraphEdgeDTO>();

        // Guarda os IDs já adicionados para evitar duplicados
        var nodeIds = new HashSet<string>();

        // Adiciona um nó ao grafo
        void AddNode(
            string id,
            string label,
            string type,
            string? subtitle = null)
        {
            if (nodeIds.Add(id))
            {
                nodes.Add(new WipGraphNodeDTO
                {
                    Id = id,
                    Label = label,
                    Type = type,
                    Subtitle = subtitle
                });
            }
        }

        // Adiciona uma ligação entre dois nós
        void AddEdge(string source, string target, string type)
        {
            edges.Add(new WipGraphEdgeDTO
            {
                Source = source,
                Target = target,
                Type = type
            });
        }

        // Nó principal da fábrica
        AddNode(
            "factory",
            "Produção",
            "factory",
            "Estado global");

        // Produtos em espera
        foreach (var item in waitingItems)
        {
            var productNode = $"product-{item.ProductId}";

            AddNode(
                productNode,
                item.SerialNumber ?? $"Produto {item.ProductId}",
                "waitingProduct",
                item.QueueReason);

            AddEdge("factory", productNode, "waiting");

            // Próxima fase prevista
            if (!string.IsNullOrWhiteSpace(item.NextPhase))
            {
                var phaseNode = $"phase-waiting-{item.ProductId}";

                AddNode(
                    phaseNode,
                    item.NextPhase,
                    "phase",
                    "próxima fase");

                AddEdge(productNode, phaseNode, "next");
            }

            // Suporte (skid) associado
            if (item.SupportId != null)
            {
                var skidNode = $"support-{item.SupportId}";

                AddNode(
                    skidNode,
                    item.RfidTag ?? $"Skid {item.SupportId}",
                    "support",
                    item.SupportType);

                AddEdge(skidNode, productNode, "carries");
            }
        }

        // Produtos atualmente em produção
        foreach (var item in inProgressItems)
        {
            var lineNode = $"line-{item.ProductionLineId}";
            var wsNode = $"ws-{item.WorkstationId}";
            var productNode = $"product-{item.ProductId}";
            var phaseNode = $"phase-active-{item.ProductId}";

            AddNode(
                lineNode,
                item.ProductionLineName ?? $"Linha {item.ProductionLineId}",
                "line");

            AddNode(
                wsNode,
                item.Workstation ?? $"WS {item.WorkstationId}",
                "workstation");

            AddNode(
                productNode,
                item.SerialNumber ?? $"Produto {item.ProductId}",
                "activeProduct",
                item.WipStatus);

            AddNode(
                phaseNode,
                item.CurrentPhase ?? "Fase atual",
                "phase",
                "em curso");

            AddEdge("factory", lineNode, "contains");
            AddEdge(lineNode, wsNode, "contains");
            AddEdge(wsNode, productNode, "working");
            AddEdge(productNode, phaseNode, "currentPhase");
        }

        // Devolve o grafo completo
        return new WipGraphDTO
        {
            Nodes = nodes,
            Edges = edges
        };
    }
}