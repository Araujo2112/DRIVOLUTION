namespace Drivolution.DTO;

// Resultado estimado para uma única fase, dentro de uma simulação de tempo de fabrico.
public record PhaseEtaSimulationDTO(int ManufacturingPhaseId, string PhaseName, int Order, int EstimatedSeconds);

// Resultado completo da simulação: modelo + opções escolhidas, sem produto real
// na BD. "EstimateIsTrained" indica se o treino de ML já correu pelo menos uma
// vez (existem coeficientes gravados) — quando false, a estimativa usa só o
// valor estático por fase (EstimatedDuration), sem nenhum ajuste por modelo,
// linha ou opções, e o frontend pode querer avisar disso.
public record EtaSimulationResultDTO(
    int ModelId,
    string ModelName,
    List<int> SelectedConfigOptionIds,
    List<PhaseEtaSimulationDTO> Phases,
    int TotalEstimatedSeconds,
    bool EstimateIsTrained,
    DateTime? CoefficientsTrainedAt
);