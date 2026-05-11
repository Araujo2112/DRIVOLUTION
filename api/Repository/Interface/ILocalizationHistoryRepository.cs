using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface.LocalizationHistory;
public interface ILocalizationHistoryRepository
{
    Task<IEnumerable<LocalizationHistoryModel>> GetBySupport(int supportId);
    Task<LocalizationHistoryModel?> GetCurrentBySupport(int supportId);
    Task<LocalizationHistoryModel> Create(LocalizationHistoryModel entity);
    Task Update(LocalizationHistoryModel entity);
}
