namespace ApiTexPact.DTO;

public record ProductPhaseDTO(int Id, int ProductId, int ManufacturingPhaseId, string PhaseName, int WorkstationId, string? Notes, string? Result, DateTime DatetimeIni, DateTime? DatetimeEnd, int? QualityId);
public record CreateProductPhaseDTO(int ProductId, int ManufacturingPhaseId, int WorkstationId, string? Notes);
public record CloseProductPhaseDTO(string? Result, int? QualityId);