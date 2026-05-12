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

// Resultado da criação de uma ClientOrder (resposta do POST)
public record CreateClientOrderResultDTO(
    int OrderId,
    string CustomerName,
    int TotalQuantity,
    List<ProductSummaryDTO> ProductsCreated
);

public record UpdateClientOrderDTO(string? OrderNumber, DateTime? OrderDate, string? CustomerName, int? Quantity);

// Resumo de cada produto criado
public record ProductSummaryDTO(
    int ProductId,
    string SerialNumber,
    string MoNumber
);
