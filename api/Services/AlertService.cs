using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Services.Interface;
using Drivolution.Repository.Interface;

namespace Drivolution.Services;

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepo;

    public AlertService(IAlertRepository alertRepo)
    {
        _alertRepo = alertRepo;
    }

    public async Task<PagedResultDTO<AlertModel>> GetPagedAsync(int page, int pageSize, string? type, string? status)
        => await _alertRepo.GetPagedAsync(page, pageSize, type, status);

    public async Task<IEnumerable<AlertModel>> GetAllAsync()
        => await _alertRepo.GetAllAsync();

    public async Task<IEnumerable<AlertModel>> GetOpenAsync()
        => await _alertRepo.GetOpenAsync();

    public async Task<AlertModel?> AcknowledgeAsync(int id)
    {
        var alert = await _alertRepo.GetByIdAsync(id);
        if (alert == null) return null;

        alert.Status = "acknowledged";
        alert.AcknowledgedAt = DateTime.UtcNow;
        return await _alertRepo.UpdateAsync(alert);
    }

    public async Task<AlertModel?> ResolveAsync(int id)
    {
        var alert = await _alertRepo.GetByIdAsync(id);
        if (alert == null) return null;

        alert.Status = "resolved";
        alert.ResolvedAt = DateTime.UtcNow;
        return await _alertRepo.UpdateAsync(alert);
    }

    public async Task<AlertModel> CreateAsync(
        string type, int productId, int productPhaseId,
        string productSerial, string phaseName,
        int? thresholdPct = null, int? estimatedDuration = null,
        int? orderFrom = null, int? orderTo = null)
    {
        var alert = new AlertModel
        {
            Type = type,
            Status = "open",
            ProductId = productId,
            ProductPhaseId = productPhaseId,
            TriggeredAt = DateTime.UtcNow,
            ProductSerial = productSerial,
            PhaseName = phaseName,
            ThresholdPct = thresholdPct,
            EstimatedDuration = estimatedDuration,
            OrderFrom = orderFrom,
            OrderTo = orderTo,
        };
        return await _alertRepo.CreateAsync(alert);
    }
}