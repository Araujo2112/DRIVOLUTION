namespace Drivolution.DTO;

public record MlTrainResultDTO(bool Success, string Output, string? Error, DateTime RanAt);