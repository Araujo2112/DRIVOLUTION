namespace ApiTexPact.DTO;

public record CarModelDTO(int Id, string Name, string? Version, string? Type);
public record CreateCarModelDTO(string Name, string? Version, string? Type);
public record UpdateCarModelDTO(string? Name, string? Version, string? Type);