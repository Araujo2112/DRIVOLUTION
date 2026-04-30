using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.Client;

public interface IClientRepository
{
    Task<IEnumerable<ClientModel>> GetAll();
    Task<ClientModel> GetById(int id);
    Task<ClientModel> Create(ClientModel client);
    Task Update(ClientModel client);
    Task Delete(int id);
    Task<bool> Exists(int id);
}