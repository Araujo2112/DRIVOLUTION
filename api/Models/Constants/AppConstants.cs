namespace ApiTexPact.Models.Constants;

    // Para a tabela ManufacturingPhase e QualityCheck
public static class AppConstants
{
    public const string None = "none";
    public const string Minor = "minor";
    public const string Major = "major";
    public const string Critical = "critical";

    // Método auxiliar para ajudar o Service a decidir
    public static int GetWeight(string severity) => severity.ToLower() switch
    {
        None => 0,
        Minor => 1,
        Major => 2,
        Critical => 3,
        _ => 99
    };
}

// Para o status do QualityCheck e resultado do ProductPhase
public static class QualityStatus
{
    public const string Passed = "passed";
    public const string Rework = "rework";
    public const string Scrapped = "scrapped";
}

// Para as outras tabelas que mencionaste (Resource, Order, etc.)
public static class EntityStatus
{
    // Resource e WorkstationAllocation
    public const string Active = "active";
    public const string Inactive = "inactive";
    
    // ManufacturingOrder
    public const string Pending = "pending";
    public const string InProgress = "in_progress";
    public const string Completed = "completed";
    public const string Cancelled = "cancelled";

    // WorkstationStatus
    public const string Functional = "functional";
    public const string Maintenance = "maintenance";
    public const string Broken = "broken";
}
