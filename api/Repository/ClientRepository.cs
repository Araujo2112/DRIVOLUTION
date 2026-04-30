using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Client;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class ClientRepository : IClientRepository
{
    private readonly ApplicationDbContext _context;

    public ClientRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ClientModel>> GetAll()
    {
        return await _context.Clients.ToListAsync();
    }

    public async Task<ClientModel> GetById(int id)
    {
        return await _context.Clients.FindAsync(id)
               ?? throw new KeyNotFoundException($"Client with ID {id} not found");
    }

    public async Task<ClientModel> Create(ClientModel client)
    {
        _context.Clients.Add(client);
        await _context.SaveChangesAsync();
        return client;
    }

    public async Task Update(ClientModel client)
    {
        _context.Entry(client).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }

    public async Task Delete(int id)
    {
        var client = await GetById(id);
        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.Clients.AnyAsync(c => c.Id == id);
    }
}