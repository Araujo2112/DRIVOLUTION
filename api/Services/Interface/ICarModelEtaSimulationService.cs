using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public enum EtaSimulationErrorCode
{
    None,
    ModelNotFound,
    OptionNotFoundForModel,
}

public class EtaSimulationResult<T>
{
    public bool Success { get; init; }
    public T? Value { get; init; }
    public EtaSimulationErrorCode ErrorCode { get; init; } = EtaSimulationErrorCode.None;
    public string? ErrorMessage { get; init; }

    public static EtaSimulationResult<T> Ok(T value) => new() { Success = true, Value = value };

    public static EtaSimulationResult<T> Fail(EtaSimulationErrorCode code, string message) =>
        new() { Success = false, ErrorCode = code, ErrorMessage = message };
}

public interface ICarModelEtaSimulationService
{
    // Simula o tempo de fabrico de um carro hipotético (modelo + opções de
    // configuração escolhidas), sem criar nenhum Product ou ManufacturingOrder
    // real na base de dados. Falha com OptionNotFoundForModel se algum
    // configOptionId não pertencer a uma Config do modelo pedido — nunca soma
    // silenciosamente zero para uma opção inválida.
    Task<EtaSimulationResult<EtaSimulationResultDTO>> Simulate(int modelId, IEnumerable<int> configOptionIds);
}