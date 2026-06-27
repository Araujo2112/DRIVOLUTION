namespace Drivolution.DTO;

public class ProductionLineStatusDTO
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