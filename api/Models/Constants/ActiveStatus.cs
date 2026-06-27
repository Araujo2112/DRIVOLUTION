namespace Drivolution.Models.Constants;

// Estado simples de "a decorrer / terminado", usado por Resource e
// WorkstationAllocation (Active/Inactive) e por LocalizationHistory
// (Active enquanto o skid está nessa estação, Completed quando sai).

public static class ActiveStatus
{
    public const string Active = "active";
    public const string Inactive = "inactive";
    public const string Completed = "completed";
}