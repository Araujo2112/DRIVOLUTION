using Drivolution.DTO;
namespace Drivolution.Services.Interface;

public interface IClientOrderService
{
    Task<PagedResultDTO<ClientOrderDTO>> GetPaged(int page, int pageSize, string? search, string? status, DateTime? dateFrom, DateTime? dateTo);
    Task<ClientOrderDTO?> GetById(int id);
    Task<CreateClientOrderResultDTO> Create(CreateClientOrderDTO dto);
    Task<bool> Update(int id, UpdateClientOrderDTO dto);
    Task<bool> Delete(int id);
    Task<bool> Cancel(int id);
}