namespace ApiTexPact.DTO;

public record WorkstationAllocationDTO(int Id, int ResourceId, bool IsHuman, int WorkstationId, string? Status, DateTime StartDate, DateTime? EndDate);
public record CreateWorkstationAllocationDTO(int ResourceId, int WorkstationId, string? Status, DateTime StartDate);
public record UpdateWorkstationAllocationDTO(string? Status, DateTime? EndDate);