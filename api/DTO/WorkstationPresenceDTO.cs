namespace Drivolution.DTO;

// Resposta de uma entrada de presença
public record WorkstationPresenceDTO(
    int Id,
    int AppUserId,
    string UserName,
    string UserEmail,
    int WorkstationId,
    string? WorkstationIdentifier,  // o campo "type" = "A", "B", etc.
    string? PhaseName,
    DateTime CheckedInAt,
    DateTime? CheckedOutAt
);

// Request body para check-in (user vem do JWT)
public record CheckInRequestDTO(
    int WorkstationId
);

// Produto que passou nesta workstation durante uma presença
public record PresenceProductCrossDTO(
    int ProductId,
    string SerialNumber,
    DateTime PhaseStart,
    DateTime? PhaseEnd
);

// Resposta enriquecida: presença + produtos cruzados
public record WorkstationPresenceDetailDTO(
    int Id,
    int AppUserId,
    string UserName,
    string UserEmail,
    int WorkstationId,
    string? WorkstationIdentifier,
    string? PhaseName,
    DateTime CheckedInAt,
    DateTime? CheckedOutAt,
    IEnumerable<PresenceProductCrossDTO> ProductsDuringPresence
);