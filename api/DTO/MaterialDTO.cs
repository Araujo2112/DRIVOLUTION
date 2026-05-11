namespace ApiTexPact.DTO;

public record MaterialDTO(int Id, string Item, string? PartNumber);
public record CreateMaterialDTO(string Item, string? PartNumber);
public record UpdateMaterialDTO(string? Item, string? PartNumber);