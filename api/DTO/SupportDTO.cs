namespace Drivolution.DTO;

public record SupportDTO(int Id, int ProductionLineId, string? RfidTag, string? Type);
public record CreateSupportDTO(int ProductionLineId, string? RfidTag, string? Type);
public record UpdateSupportDTO(string? RfidTag, string? Type);