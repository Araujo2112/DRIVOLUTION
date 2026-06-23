namespace Drivolution.DTO;

public record ProductionLineDTO(int Id, string Name, string? Location, string? Status, int? Capacity);
public record CreateProductionLineDTO(string Name, string? Location, string? Status, int? Capacity);
public record UpdateProductionLineDTO(string? Name, string? Location, string? Status, int? Capacity);