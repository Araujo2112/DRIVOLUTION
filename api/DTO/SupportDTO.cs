namespace Drivolution.DTO;

public record SupportDTO(int Id, int ProductionLineId, string? RfidTag, string? Type);
public record CreateSupportDTO(int ProductionLineId, string? RfidTag, string? Type);
public record UpdateSupportDTO(string? RfidTag, string? Type);

// Suporte com estado já calculado (evita N+1 pedidos no frontend na listagem paginada)
public record SupportPagedDTO(
    int Id,
    int ProductionLineId,
    string ProductionLineName,
    string? RfidTag,
    string? Type,
    bool IsOccupied,
    int? CurrentProductId,
    string? CurrentSerialNumber,
    string? CurrentModelName
);