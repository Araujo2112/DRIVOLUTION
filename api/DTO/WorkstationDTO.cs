namespace Drivolution.DTO;

public record WorkstationDTO(
    int Id,
    int ProductionLineId,
    string? ProductionLineName,
    string? Type,
    string? Kind,
    int? ManufacturingPhaseId,
    string? PhaseName
);

public record CreateWorkstationDTO(
    int ProductionLineId,
    string? Type,
    string? Kind,
    int? ManufacturingPhaseId
);

public record UpdateWorkstationDTO(
    string? Type,
    string? Kind,
    int? ManufacturingPhaseId
);