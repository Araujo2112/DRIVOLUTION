using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface;
public interface ILocalizationHistoryRepository
{
    Task<IEnumerable<LocalizationHistoryModel>> GetBySupport(int supportId);
    Task<LocalizationHistoryModel?> GetCurrentBySupport(int supportId);
    Task<LocalizationHistoryModel> Create(LocalizationHistoryModel entity);
    Task Update(LocalizationHistoryModel entity);
}
