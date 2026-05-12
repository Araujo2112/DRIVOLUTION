using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface;

public interface IClientOrderService
{
    Task<IEnumerable<ClientOrderDTO>> GetAll();
    Task<ClientOrderDTO?> GetById(int id);
    Task<CreateClientOrderResultDTO> Create(CreateClientOrderDTO dto);
    Task<bool> Update(int id, UpdateClientOrderDTO dto);
    Task<bool> Delete(int id);
}