namespace Drivolution.DTO.Analytics;

public class PhaseDurationDTO
{
    public int ManufacturingPhaseId { get; set; }
    public string PhaseName { get; set; } = string.Empty;
    public double AverageDurationMinutes { get; set; }
    public int CompletedCount { get; set; }
}