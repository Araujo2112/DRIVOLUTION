namespace ApiTexPact.DTO;

public record WorkstationDTO(int Id, int ProductionLineId, string? ProductionLineName, string? Type);
public record CreateWorkstationDTO(int ProductionLineId, string? Type);
public record UpdateWorkstationDTO(string? Type);