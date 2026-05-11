namespace ApiTexPact.DTO;

public record ManufacturingPhaseDTO(int Id, string Name, int? EstimatedDuration);
public record CreateManufacturingPhaseDTO(string Name, int? EstimatedDuration);
public record UpdateManufacturingPhaseDTO(string? Name, int? EstimatedDuration);