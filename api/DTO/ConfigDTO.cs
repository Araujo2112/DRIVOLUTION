namespace ApiTexPact.DTO;

public record ConfigDTO(int Id, int ModelId, string Item, bool AllowMultiple);
public record CreateConfigDTO(int ModelId, string Item, bool AllowMultiple = false);
public record UpdateConfigDTO(string? Item, bool? AllowMultiple);