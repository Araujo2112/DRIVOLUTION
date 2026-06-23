namespace Drivolution.DTO;

public record ManufacturingPhaseDTO(int Id, string Name, int? EstimatedDuration, string MaxAcceptableSeverity, string ReworkSeverity);
public record CreateManufacturingPhaseDTO(string Name, int? EstimatedDuration, string MaxAcceptableSeverity, string ReworkSeverity);
public record UpdateManufacturingPhaseDTO(string? Name, int? EstimatedDuration, string? MaxAcceptableSeverity, string? ReworkSeverity);