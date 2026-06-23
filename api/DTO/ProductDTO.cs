namespace Drivolution.DTO;

public record ProductDTO(int Id, int ManufacturingOrderId, int ModelId, string? ModelName, string? SerialNumber, string? LotNumber, DateTime? ProductionDate);
public record CreateProductDTO(int ManufacturingOrderId, int ModelId, string? SerialNumber, string? LotNumber);
public record UpdateProductDTO(DateTime? ProductionDate);