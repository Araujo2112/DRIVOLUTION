using System.Diagnostics;
using Drivolution.DTO;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável por executar o treino do modelo de Machine Learning
public class ModelTrainingService : IModelTrainingService
{
    // Logger utilizado para registar informação e erros
    private readonly ILogger<ModelTrainingService> _logger;

    // O ASP.NET injeta automaticamente o logger
    public ModelTrainingService(ILogger<ModelTrainingService> logger)
    {
        _logger = logger;
    }

    // Executa o script Python responsável pelo treino do modelo
    public async Task<MlTrainResultDTO> RunTraining()
    {
        // Configura o processo que irá executar o script Python
        var psi = new ProcessStartInfo
        {
            // Executável a utilizar
            FileName = "python3",

            // Caminho para o script de treino
            Arguments = "/app/ml/train_phase_time_model.py",

            // Permite ler o texto produzido pelo script
            RedirectStandardOutput = true,

            // Permite ler mensagens de erro
            RedirectStandardError = true,

            // Executa diretamente o processo, sem abrir uma consola
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        // Cria o processo utilizando a configuração anterior
        using var process = new Process
        {
            StartInfo = psi
        };

        // Inicia a execução do script
        process.Start();

        // Lê toda a saída produzida pelo script
        var output = await process.StandardOutput.ReadToEndAsync();

        // Lê todas as mensagens de erro
        var error = await process.StandardError.ReadToEndAsync();

        // Aguarda até o processo terminar
        await process.WaitForExitAsync();

        // O treino é considerado bem sucedido se o processo terminar com ExitCode = 0
        var success = process.ExitCode == 0;

        if (!success)
        {
            // Regista o erro nos logs
            _logger.LogError(
                "Falha ao treinar o modelo de ML (exit code {Code}): {Error}",
                process.ExitCode,
                error
            );
        }
        else
        {
            // Regista o sucesso e a informação devolvida pelo script
            _logger.LogInformation(
                "Modelo de ML treinado com sucesso.\n{Output}",
                output
            );
        }

        // Devolve o resultado da execução
        return new MlTrainResultDTO(
            success,
            output,
            success ? null : error,
            DateTime.UtcNow
        );
    }
}