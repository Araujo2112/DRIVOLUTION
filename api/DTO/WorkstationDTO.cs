namespace Drivolution.DTO;

public record WorkstationDTO(
    int Id,
    int ProductionLineId,
    string? ProductionLineName,
    string? Type,
    int? ManufacturingPhaseId,
    string? PhaseName
);

public record CreateWorkstationDTO(
    int ProductionLineId,
    string? Type,
    int? ManufacturingPhaseId
);

public record UpdateWorkstationDTO(
    string? Type,
    int? ManufacturingPhaseId
);