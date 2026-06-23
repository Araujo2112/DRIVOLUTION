using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface ILocalizationHistoryService
{
    Task<IEnumerable<LocalizationHistoryDTO>> GetBySupport(int supportId);
    Task<LocalizationHistoryDTO?> GetCurrent(int supportId);
    Task<LocalizationHistoryDTO> Create(CreateLocalizationHistoryDTO dto);
}
