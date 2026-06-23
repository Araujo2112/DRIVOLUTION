namespace Drivolution.DTO;

public record WorkstationStatusDTO(int Id, int WorkstationId, string Status, DateTime Timestamp);
public record CreateWorkstationStatusDTO(int WorkstationId, string Status);