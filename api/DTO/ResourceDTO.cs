namespace Drivolution.DTO;

public record ResourceDTO(int Id, bool IsHuman, string? Status);
public record CreateResourceDTO(bool IsHuman, string? Status);
public record UpdateResourceDTO(bool? IsHuman, string? Status);