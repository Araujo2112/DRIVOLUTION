using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class AlertBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AlertBackgroundService> _logger;

    public AlertBackgroundService(IServiceScopeFactory scopeFactory, ILogger<AlertBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var alertRepo = scope.ServiceProvider.GetRequiredService<IAlertRepository>();
                var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                var phaseRepo = scope.ServiceProvider.GetRequiredService<IProductPhaseRepository>();

                await CheckTimeExceeded(alertRepo, alertService, phaseRepo);
                await ResolveClosedPhaseAlerts(alertRepo, phaseRepo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro no AlertBackgroundService");
            }

            await Task.Delay(TimeSpan.FromMinutes(2), ct);
        }
    }

    private async Task CheckTimeExceeded(IAlertRepository alertRepo, IAlertService alertService, IProductPhaseRepository phaseRepo)
    {
        var openPhases = await phaseRepo.GetOpenPhasesWithPhaseInfoAsync();

        foreach (var phase in openPhases)
        {
            var estimated = phase.ManufacturingPhase?.EstimatedDuration;
            var threshold = phase.ManufacturingPhase?.TimeThresholdPct ?? 150;

            if (estimated == null || estimated == 0) continue;

            var elapsedSeconds = (DateTime.Now - phase.DatetimeIni).TotalSeconds;
            var limitSeconds = estimated.Value * (threshold / 100.0);

            if (elapsedSeconds > limitSeconds)
            {
                var alreadyExists = await alertRepo.ExistsOpenForPhaseAsync(phase.Id, "time_exceeded");
                if (!alreadyExists)
                {
                    await alertService.CreateAsync(
                        type: "time_exceeded",
                        productId: phase.ProductId,
                        productPhaseId: phase.Id,
                        productSerial: phase.Product?.SerialNumber ?? phase.ProductId.ToString(),
                        phaseName: phase.ManufacturingPhase?.Name ?? "?",
                        thresholdPct: threshold,
                        estimatedDuration: estimated.Value
                    );
                }
            }
        }
    }

    private async Task ResolveClosedPhaseAlerts(IAlertRepository alertRepo, IProductPhaseRepository phaseRepo)
    {
        var openAlerts = await alertRepo.GetOpenByTypeAsync("time_exceeded");

        foreach (var alert in openAlerts)
        {
            var phase = await phaseRepo.GetByIdAsync(alert.ProductPhaseId);
            if (phase?.DatetimeEnd != null)
            {
                alert.Status = "resolved";
                alert.ResolvedAt = DateTime.Now;
                await alertRepo.UpdateAsync(alert);
            }
        }
    }
}