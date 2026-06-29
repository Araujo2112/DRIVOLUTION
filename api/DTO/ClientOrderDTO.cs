namespace Drivolution.DTO;

// CustomerName (texto livre) substituído por ClientName, que vem do JOIN com
// app_user (AppUserId) — a encomenda está agora sempre ligada a uma conta real.
public record ClientOrderDTO(int Id, string OrderNumber, DateTime OrderDate, int AppUserId, string ClientName, int Quantity);

public record ConfigSelectionDTO(int ConfigOptionId);

public record CreateClientOrderDTO(
    string OrderNumber, 
    DateTime OrderDate, 
    int AppUserId, 
    int Quantity, 
    int ModelId, 
    List<ConfigSelectionDTO>? Configs
);

// Resultado da criação de uma ClientOrder (resposta do POST)
public record CreateClientOrderResultDTO(
    int OrderId,
    string ClientName,
    int TotalQuantity,
    List<ProductSummaryDTO> ProductsCreated
);

public record UpdateClientOrderDTO(string? OrderNumber, DateTime? OrderDate, int? AppUserId, int? Quantity);

// Resumo de cada produto criado
public record ProductSummaryDTO(
    int ProductId,
    string SerialNumber,
    string MoNumber
);