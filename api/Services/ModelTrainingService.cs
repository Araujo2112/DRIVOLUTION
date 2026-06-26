using System.Diagnostics;
using Drivolution.DTO;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ModelTrainingService : IModelTrainingService
{
    private readonly ILogger<ModelTrainingService> _logger;

    public ModelTrainingService(ILogger<ModelTrainingService> logger)
    {
        _logger = logger;
    }

    public async Task<MlTrainResultDTO> RunTraining()
    {
        var psi = new ProcessStartInfo
        {
            FileName = "python3",
            Arguments = "/app/ml/train_phase_time_model.py",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var success = process.ExitCode == 0;

        if (!success)
            _logger.LogError("Falha ao treinar o modelo de ML (exit code {Code}): {Error}", process.ExitCode, error);
        else
            _logger.LogInformation("Modelo de ML treinado com sucesso.\n{Output}", output);

        return new MlTrainResultDTO(success, output, success ? null : error, DateTime.UtcNow);
    }
}