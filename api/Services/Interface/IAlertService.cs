using Drivolution.DTO;
using Drivolution.Models;

namespace Drivolution.Services.Interface;

public interface IAlertService
{
    Task<PagedResultDTO<AlertModel>> GetPagedAsync(int page, int pageSize, string? type, string? status);
    Task<IEnumerable<AlertModel>> GetAllAsync();
    Task<IEnumerable<AlertModel>> GetOpenAsync();
    Task<AlertModel?> AcknowledgeAsync(int id);
    Task<AlertModel?> ResolveAsync(int id);

    Task<AlertModel> CreateAsync(
        string type,
        int productId,
        int productPhaseId,
        string productSerial,
        string phaseName,
        int? thresholdPct = null,
        int? estimatedDuration = null,
        int? orderFrom = null,
        int? orderTo = null
    );
}