namespace ApiTexPact.DTO;

public record ManufacturingOrderDTO(int Id, int ClientOrderId, string CustomerName, string ManufacturingOrderNumber, DateTime StartDate, DateTime? EndDate, string? Status);
public record CreateManufacturingOrderDTO(int ClientOrderId, string ManufacturingOrderNumber, DateTime StartDate);
public record UpdateManufacturingOrderDTO(string? Status, DateTime? EndDate);