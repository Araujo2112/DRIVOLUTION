using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Serviço executado automaticamente em segundo plano enquanto a API está ativa
public class AlertBackgroundService : BackgroundService
{
    // Permite criar um scope para obter services e repositories com ciclo de vida Scoped
    private readonly IServiceScopeFactory _scopeFactory;

    // Logger utilizado para registar erros e informação do serviço
    private readonly ILogger<AlertBackgroundService> _logger;

    // O ASP.NET injeta automaticamente as dependências necessárias
    public AlertBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AlertBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    // Método principal executado continuamente em segundo plano
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Continua a executar enquanto a aplicação não estiver a ser encerrada
        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Cria um scope temporário para esta execução
                using var scope = _scopeFactory.CreateScope();

                // Obtém o repository dos alertas dentro do scope
                var alertRepo =
                    scope.ServiceProvider.GetRequiredService<IAlertRepository>();

                // Obtém o service responsável pela criação de alertas
                var alertService =
                    scope.ServiceProvider.GetRequiredService<IAlertService>();

                // Obtém o repository das fases dos produtos
                var phaseRepo =
                    scope.ServiceProvider.GetRequiredService<IProductPhaseRepository>();

                // Verifica se existem fases abertas há mais tempo do que o permitido
                await CheckTimeExceeded(alertRepo, alertService, phaseRepo);

                // Resolve alertas associados a fases que entretanto já terminaram
                await ResolveClosedPhaseAlerts(alertRepo, phaseRepo);
            }
            catch (Exception ex)
            {
                // Regista o erro, mas não termina o serviço
                _logger.LogError(ex, "Erro no AlertBackgroundService");
            }

            // Aguarda dois minutos antes de voltar a executar as verificações
            await Task.Delay(TimeSpan.FromMinutes(2), ct);
        }
    }

    // Verifica se alguma fase aberta ultrapassou o tempo máximo permitido
    private async Task CheckTimeExceeded(
        IAlertRepository alertRepo,
        IAlertService alertService,
        IProductPhaseRepository phaseRepo)
    {
        // Obtém todas as fases que ainda estão abertas,
        // juntamente com a informação da fase e do produto
        var openPhases =
            await phaseRepo.GetOpenPhasesWithPhaseInfoAsync();

        // Analisa cada fase aberta
        foreach (var phase in openPhases)
        {
            // Duração estimada da fase, em segundos
            var estimated =
                phase.ManufacturingPhase?.EstimatedDuration;

            // Percentagem limite permitida.
            // Se não estiver definida, assume 150%.
            var threshold =
                phase.ManufacturingPhase?.TimeThresholdPct ?? 150;

            // Se a fase não tiver duração estimada válida, ignora-a
            if (estimated == null || estimated == 0)
                continue;

            // Calcula há quantos segundos a fase está aberta
            var elapsedSeconds =
                (DateTime.UtcNow - phase.DatetimeIni).TotalSeconds;

            // Calcula o limite máximo permitido
            // Exemplo: 1800 segundos × 150% = 2700 segundos
            var limitSeconds =
                estimated.Value * (threshold / 100.0);

            // Se o tempo decorrido ultrapassou o limite
            if (elapsedSeconds > limitSeconds)
            {
                // Verifica se já existe um alerta aberto ou reconhecido
                // para esta fase, evitando alertas duplicados
                var alreadyExists =
                    await alertRepo.ExistsOpenForPhaseAsync(
                        phase.Id,
                        "time_exceeded"
                    );

                if (!alreadyExists)
                {
                    // Cria um novo alerta de tempo excedido
                    await alertService.CreateAsync(
                        type: "time_exceeded",
                        productId: phase.ProductId,
                        productPhaseId: phase.Id,
                        productSerial:
                            phase.Product?.SerialNumber
                            ?? phase.ProductId.ToString(),
                        phaseName:
                            phase.ManufacturingPhase?.Name
                            ?? "?",
                        thresholdPct: threshold,
                        estimatedDuration: estimated.Value
                    );
                }
            }
        }
    }

    // Resolve os alertas de tempo excedido quando a respetiva fase já foi fechada
    private async Task ResolveClosedPhaseAlerts(
        IAlertRepository alertRepo,
        IProductPhaseRepository phaseRepo)
    {
        // Obtém os alertas de tempo excedido que ainda estão
        // abertos ou reconhecidos
        var openAlerts =
            await alertRepo.GetOpenByTypeAsync("time_exceeded");

        // Verifica cada alerta
        foreach (var alert in openAlerts)
        {
            // Obtém a fase associada ao alerta
            var phase =
                await phaseRepo.GetByIdAsync(alert.ProductPhaseId);

            // Se a fase já tiver uma data de fim,
            // significa que o problema deixou de estar ativo
            if (phase?.DatetimeEnd != null)
            {
                // Marca o alerta como resolvido
                alert.Status = "resolved";

                // Regista o momento da resolução
                alert.ResolvedAt = DateTime.UtcNow;

                // Guarda as alterações
                await alertRepo.UpdateAsync(alert);
            }
        }
    }
}