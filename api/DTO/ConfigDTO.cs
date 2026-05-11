namespace ApiTexPact.DTO;

public record ConfigDTO(int Id, int ModelId, string Item);
public record CreateConfigDTO(int ModelId, string Item);
public record UpdateConfigDTO(string? Item);

