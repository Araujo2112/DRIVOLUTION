using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Services.Interface;
using Drivolution.Repository.Interface;

namespace Drivolution.Services;

// Implementa o contrato definido em IAlertService
public class AlertService : IAlertService
{
    // Repository responsável por consultar e guardar alertas
    private readonly IAlertRepository _alertRepo;

    // O ASP.NET injeta automaticamente o repository
    public AlertService(IAlertRepository alertRepo)
    {
        _alertRepo = alertRepo;
    }

    // Devolve uma lista paginada de alertas,
    // podendo aplicar filtros por tipo e estado
    public async Task<PagedResultDTO<AlertModel>> GetPagedAsync(
        int page,
        int pageSize,
        string? type,
        string? status)
        => await _alertRepo.GetPagedAsync(
            page,
            pageSize,
            type,
            status
        );

    // Devolve todos os alertas
    public async Task<IEnumerable<AlertModel>> GetAllAsync()
        => await _alertRepo.GetAllAsync();

    // Devolve apenas os alertas que ainda estão abertos
    public async Task<IEnumerable<AlertModel>> GetOpenAsync()
        => await _alertRepo.GetOpenAsync();

    // Marca um alerta como reconhecido (acknowledged)
    public async Task<AlertModel?> AcknowledgeAsync(int id)
    {
        // Procura o alerta pelo ID
        var alert = await _alertRepo.GetByIdAsync(id);

        // Se não existir devolve null
        if (alert == null)
            return null;

        // Atualiza o estado do alerta
        alert.Status = "acknowledged";

        // Guarda a data e hora em que foi reconhecido
        alert.AcknowledgedAt = DateTime.UtcNow;

        // Guarda as alterações na base de dados
        return await _alertRepo.UpdateAsync(alert);
    }

    // Marca um alerta como resolvido
    public async Task<AlertModel?> ResolveAsync(int id)
    {
        // Procura o alerta
        var alert = await _alertRepo.GetByIdAsync(id);

        // Se não existir devolve null
        if (alert == null)
            return null;

        // Atualiza o estado para resolvido
        alert.Status = "resolved";

        // Guarda a data e hora da resolução
        alert.ResolvedAt = DateTime.UtcNow;

        // Atualiza o alerta na base de dados
        return await _alertRepo.UpdateAsync(alert);
    }

    // Cria um novo alerta
    public async Task<AlertModel> CreateAsync(
        string type,
        int productId,
        int productPhaseId,
        string productSerial,
        string phaseName,
        int? thresholdPct = null,
        int? estimatedDuration = null,
        int? orderFrom = null,
        int? orderTo = null)
    {
        // Cria a entidade que será guardada na base de dados
        var alert = new AlertModel
        {
            Type = type,

            // Todos os alertas começam com o estado "open"
            Status = "open",

            ProductId = productId,
            ProductPhaseId = productPhaseId,

            // Guarda a data e hora em que o alerta foi criado
            TriggeredAt = DateTime.UtcNow,

            ProductSerial = productSerial,
            PhaseName = phaseName,

            // Informação adicional do alerta (opcional)
            ThresholdPct = thresholdPct,
            EstimatedDuration = estimatedDuration,
            OrderFrom = orderFrom,
            OrderTo = orderTo,
        };

        // Guarda o alerta na base de dados
        return await _alertRepo.CreateAsync(alert);
    }
}