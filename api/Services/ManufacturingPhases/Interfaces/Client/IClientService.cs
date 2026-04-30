using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.Client;

public interface IClientService
{
    Task<IEnumerable<ClientDTO>> GetAllClients();
    Task<ClientDTO> GetClientById(int id);
    Task<ClientDTO> CreateClient(CreateClientDTO clientDto);
    Task<ClientDTO> UpdateClient(int id, UpdateClientDTO clientDto);
    Task DeleteClient(int id);
}