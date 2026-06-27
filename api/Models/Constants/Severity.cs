namespace Drivolution.Models.Constants;


// Níveis de gravidade usados em ManufacturingPhase (limites aceitáveis)
// e QualityCheck (gravidade observada num controlo de qualidade).

public static class Severity
{
    public const string None = "none";
    public const string Minor = "minor";
    public const string Major = "major";
    public const string Critical = "critical";

    //Converte a gravidade em peso numérico para comparação (none=0 ... critical=3).
    public static int GetWeight(string severity) => severity.ToLower() switch
    {
        "none" => 0,
        "minor" => 1,
        "major" => 2,
        "critical" => 3,
        _ => 99
    };
}