namespace ApiTexPact.DTO;

public record SupportedProductDTO(
    int Id,
    int SupportId,
    int? ProductId,
    string? SerialNumber,
    string? ModelName,
    DateTime DatetimeIni,
    DateTime? DatetimeEnd
);

public record CreateSupportedProductDTO(
    int SupportId,
    int ProductId
);