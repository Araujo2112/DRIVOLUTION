namespace ApiTexPact.DTO;

public record LocalizationHistoryDTO(int Id, int SupportId, int WorkstationId, string? WorkstationType, DateTime DatetimeIni, DateTime? DatetimeEnd, string? Status);
public record CreateLocalizationHistoryDTO(int SupportId, int WorkstationId);
public record CloseLocalizationHistoryDTO(string? Status);