namespace Drivolution.DTO.Analytics;

public class ReworkRateDTO
{
    public int ManufacturingPhaseId { get; set; }
    public string PhaseName { get; set; } = string.Empty;
    public int TotalChecks { get; set; }
    public int FailedChecks { get; set; }
    public double ReworkRatePercent { get; set; }
}