namespace Drivolution.DTO;

public record EtaResultDTO(
    int ProductId,
    string SerialNumber,
    DateTime EstimatedCompletion,
    string CurrentPhase,
    int RemainingSeconds,
    DateTime? ModelTrainedAt
);