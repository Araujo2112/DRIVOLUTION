using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Client;
using ApiTexPact.Services.Interface.Client;

namespace ApiTexPact.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ClientDTO>> GetAllClients()
    {
        var clients = await _repository.GetAll();
        return clients.Select(ToDTO);
    }

    public async Task<ClientDTO> GetClientById(int id)
    {
        var client = await _repository.GetById(id);
        return ToDTO(client);
    }

    public async Task<ClientDTO> CreateClient(CreateClientDTO clientDto)
    {
        var client = new ClientModel
        {
            Name = clientDto.Name,
            FiscalNumber = clientDto.FiscalNumber
        };

        var created = await _repository.Create(client);
        return ToDTO(created);
    }

    public async Task<ClientDTO> UpdateClient(int id, UpdateClientDTO clientDto)
    {
        var existing = await _repository.GetById(id);

        existing.Name = clientDto.Name;
        existing.FiscalNumber = clientDto.FiscalNumber;

        await _repository.Update(existing);
        return ToDTO(existing);
    }

    public async Task DeleteClient(int id)
    {
        await _repository.Delete(id);
    }

    private static ClientDTO ToDTO(ClientModel client)
    {
        return new ClientDTO
        {
            Id = client.Id,
            Name = client.Name,
            FiscalNumber = client.FiscalNumber
        };
    }
}