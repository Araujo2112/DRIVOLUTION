namespace ApiTexPact.DTO;

public record QualityCheckDTO(int Id, int ProductId, int ManufacturingPhaseId, string? Notes, string? Status);
public record CreateQualityCheckDTO(int ProductId, int ManufacturingPhaseId, string? Notes, string? Status);
public record UpdateQualityCheckDTO(string? Notes, string? Status);