namespace ApiTexPact.DTO;

public class WaitingItemDTO
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