using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class MlRetrainBackgroundService : BackgroundService
{
    private readonly IModelTrainingService _trainingService;
    private readonly ILogger<MlRetrainBackgroundService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromDays(7);

    public MlRetrainBackgroundService(IModelTrainingService trainingService, ILogger<MlRetrainBackgroundService> logger)
    {
        _trainingService = trainingService;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("A iniciar retreino agendado do modelo de ML...");
                await _trainingService.RunTraining();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no retreino agendado do modelo de ML.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }
}