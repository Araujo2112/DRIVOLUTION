namespace Drivolution.DTO;

public class WipDashboardResultDTO
{
    public int TotalProducts { get; set; }
    public int Waiting { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int ActiveLines { get; set; }
    public List<WaitingItemDTO> WaitingItems { get; set; } = [];
    public List<WipItemDTO> Items { get; set; } = [];
    public WipGraphDTO Graph { get; set; } = new();
}

public class WipGraphDTO
{
    public List<WipGraphNodeDTO> Nodes { get; set; } = [];
    public List<WipGraphEdgeDTO> Edges { get; set; } = [];
}