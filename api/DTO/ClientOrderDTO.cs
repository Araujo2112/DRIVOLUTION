namespace ApiTexPact.DTO;

public record ClientOrderDTO(int Id, string OrderNumber, DateTime OrderDate, string CustomerName, int Quantity);

public record ConfigSelectionDTO(int ConfigOptionId);

public record CreateClientOrderDTO(
    string OrderNumber, 
    DateTime OrderDate, 
    string CustomerName, 
    int Quantity, 
    int ModelId, 
    List<ConfigSelectionDTO>? Configs
);

public record UpdateClientOrderDTO(string? OrderNumber, DateTime? OrderDate, string? CustomerName, int? Quantity);

public record ProductConfigInputDTO(int ConfigId, string? Value);
