using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Background Service responsável por executar automaticamente o retreino
// periódico do modelo de Machine Learning
public class MlRetrainBackgroundService : BackgroundService
{
    // Service responsável por executar o processo de treino
    private readonly IModelTrainingService _trainingService;

    // Logger utilizado para registar informação e erros
    private readonly ILogger<MlRetrainBackgroundService> _logger;

    // Intervalo entre cada retreino (7 dias)
    private static readonly TimeSpan Interval = TimeSpan.FromDays(7);

    // O ASP.NET injeta automaticamente o service e o logger
    public MlRetrainBackgroundService(
        IModelTrainingService trainingService,
        ILogger<MlRetrainBackgroundService> logger)
    {
        _trainingService = trainingService;
        _logger = logger;
    }

    // Método executado automaticamente quando a aplicação inicia
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // O ciclo mantém-se ativo enquanto a aplicação estiver em execução
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Regista nos logs o início do retreino
                _logger.LogInformation(
                    "A iniciar retreino agendado do modelo de ML..."
                );

                // Executa o processo de treino do modelo
                await _trainingService.RunTraining();
            }
            catch (Exception ex)
            {
                // Caso ocorra algum erro durante o treino,
                // este é registado mas não interrompe o serviço
                _logger.LogError(
                    ex,
                    "Erro no retreino agendado do modelo de ML."
                );
            }

            // Aguarda o intervalo definido antes de executar novamente
            await Task.Delay(Interval, stoppingToken);
        }
    }
}