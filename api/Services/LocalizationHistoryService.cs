using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;

namespace ApiTexPact.Services;

public class LocalizationHistoryService : ILocalizationHistoryService
{
    private readonly ILocalizationHistoryRepository _repo;

    public LocalizationHistoryService(ILocalizationHistoryRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<LocalizationHistoryDTO>> GetBySupport(int supportId)
    {
        var items = await _repo.GetBySupport(supportId);
        return items.Select(ToDTO);
    }

    public async Task<LocalizationHistoryDTO?> GetCurrent(int supportId)
    {
        var item = await _repo.GetCurrentBySupport(supportId);
        return item == null ? null : ToDTO(item);
    }

    public async Task<LocalizationHistoryDTO> Create(CreateLocalizationHistoryDTO dto)
    {
        // Fechar a localização ativa antes de abrir uma nova
        var current = await _repo.GetCurrentBySupport(dto.SupportId);
        if (current != null)
        {
            current.DatetimeEnd = DateTime.UtcNow;
            current.Status = "completed";
            await _repo.Update(current);
        }

        var entity = new LocalizationHistoryModel
        {
            SupportId = dto.SupportId,
            WorkstationId = dto.WorkstationId,
            DatetimeIni = DateTime.UtcNow,
            Status = "active"
        };

        var created = await _repo.Create(entity);
        return ToDTO(created);
    }

    // --- Auxiliar ---
    private static LocalizationHistoryDTO ToDTO(LocalizationHistoryModel lh) =>
        new(lh.Id, lh.SupportId, lh.WorkstationId, lh.Workstation?.Type,
            lh.DatetimeIni, lh.DatetimeEnd, lh.Status);
}
