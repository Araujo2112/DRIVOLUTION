namespace ApiTexPact.DTO;

public class ProductTimelineDTO
{
    public int ProductPhaseId { get; set; }
    public int ProductId { get; set; }
    public int ModelId { get; set; }
    public string? SerialNumber { get; set; }
    public int ManufacturingPhaseId { get; set; }
    public string? PhaseName { get; set; }
    public string? Workstation { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public int? DurationSeconds { get; set; }
    public string? Result { get; set; }
    public string? Notes { get; set; }
}