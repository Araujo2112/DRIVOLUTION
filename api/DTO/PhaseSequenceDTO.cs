namespace ApiTexPact.DTO;

public record PhaseSequenceDTO(int Id, int Order, int ManufacturingPhaseId, string PhaseName, int ModelId);
public record CreatePhaseSequenceDTO(int Order, int ManufacturingPhaseId, int ModelId);
public record UpdatePhaseSequenceDTO(int? Order);