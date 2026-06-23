namespace Drivolution.DTO;

public record QualityCheckDTO(int Id, int ProductId, int ManufacturingPhaseId, string? Notes, string? Status, string? Severity);
public record CreateQualityCheckDTO(int ProductId, int ManufacturingPhaseId, string? Notes, string? Status, string Severity);
public record UpdateQualityCheckDTO(string? Notes, string? Status);