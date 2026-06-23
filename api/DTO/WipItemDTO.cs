namespace Drivolution.DTO;

public class WipItemDTO
{
    public int ProductId { get; set; }
    public string? SerialNumber { get; set; }
    public int ProductionLineId { get; set; }
    public string? ProductionLineName { get; set; }
    public int WorkstationId { get; set; }
    public string? Workstation { get; set; }
    public string? CurrentPhase { get; set; }
    public int? EstimatedDuration { get; set; }
    public int? TimeThresholdPct { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public string? Result { get; set; }
    public string? WipStatus { get; set; }
    public int ElapsedSeconds { get; set; }
}