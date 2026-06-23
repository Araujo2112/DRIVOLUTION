using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface ILocalizationHistoryRepository
{
    Task<IEnumerable<LocalizationHistoryModel>> GetBySupport(int supportId);
    Task<LocalizationHistoryModel?> GetCurrentBySupport(int supportId);
    Task<LocalizationHistoryModel> Create(LocalizationHistoryModel entity);
    Task Update(LocalizationHistoryModel entity);
}
