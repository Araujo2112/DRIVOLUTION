namespace Drivolution.DTO;

public record ManufacturingOrderDTO(int Id, int ClientOrderId, string ClientName, string ManufacturingOrderNumber, DateTime StartDate, DateTime? EndDate, string? Status);
public record CreateManufacturingOrderDTO(int ClientOrderId, string ManufacturingOrderNumber, DateTime StartDate);
public record UpdateManufacturingOrderDTO(string? Status, DateTime? EndDate);

// Detalhe completo de uma MO — usado em GET /api/ManufacturingOrder/{id}/details
public record ManufacturingOrderDetailDTO(
    int Id,
    int ClientOrderId,
    string ClientName,
    string ManufacturingOrderNumber,
    DateTime StartDate,
    DateTime? EndDate,
    string? Status,
    List<ProductDetailDTO> Products
);

// Produto com as suas configurações e fases
public record ProductDetailDTO(
    int Id,
    string? SerialNumber,
    string? LotNumber,
    string? ModelName,
    DateTime? ProductionDate,
    List<ProductConfigDetailDTO> Configs,
    List<ProductPhaseDetailDTO> Phases
);

// Configuração aplicada ao produto (ex: Cor = Vermelho)
// ConfigItem = campo "item" do ConfigModel (ex: "Cor")
// OptionValue = campo "value" do ConfigOptionModel (ex: "Vermelho")
public record ProductConfigDetailDTO(
    int ConfigOptionId,
    string ConfigItem,
    string OptionValue
);

// Fase de produção do produto
public record ProductPhaseDetailDTO(
    int Id,
    string? PhaseName,
    DateTime DatetimeIni,
    DateTime? DatetimeEnd,
    string? Result,
    string? Notes
);