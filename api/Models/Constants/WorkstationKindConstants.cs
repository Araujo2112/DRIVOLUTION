namespace Drivolution.Models.Constants;

// Classificação do tipo de operação de uma workstation.
// Usado para filtrar workstations elegíveis para presença humana (Card L).
// Valores persistidos em workstation.kind (coluna separada de workstation.type,
// que é usado como identificador de posto, ex: "A", "B", "1", "2"...).
public static class WorkstationKind
{
    public const string Human  = "human";
    public const string Hybrid = "hybrid";
    public const string Machine = "machine";

    public static readonly string[] HumanEligible = { Human, Hybrid };
}