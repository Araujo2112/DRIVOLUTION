namespace Drivolution.DTO;

public record ConfigOptionDTO(int Id, int ConfigId, string Value, bool IsDefault);
public record CreateConfigOptionDTO(int ConfigId, string Value, bool IsDefault);