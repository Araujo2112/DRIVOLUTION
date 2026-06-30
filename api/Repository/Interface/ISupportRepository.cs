using Drivolution.DTO;
using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface ISupportRepository
{
    Task<PagedResultDTO<SupportPagedDTO>> GetPaged(int page, int pageSize, string? search, int? productionLineId, bool? occupied);
    Task<IEnumerable<SupportModel>> GetAll();
    Task<SupportModel?> GetById(int id);
    Task<SupportModel?> GetByRfidTag(string rfidTag);
    Task<SupportModel> Create(SupportModel entity);
    Task Update(SupportModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}