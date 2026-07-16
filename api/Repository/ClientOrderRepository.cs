using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por todas as operações relacionadas com as encomendas dos clientes
public class ClientOrderRepository : IClientOrderRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public ClientOrderRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as encomendas dos clientes,
    // incluindo o cliente e as ordens de fabrico associadas
    public async Task<IEnumerable<ClientOrderModel>> GetAll() =>
        await _context.ClientOrders
            .Include(c => c.AppUser)
            .Include(c => c.ManufacturingOrders)
            .ToListAsync();

    // Procura uma encomenda pelo seu ID,
    // incluindo o cliente e as ordens de fabrico associadas
    public async Task<ClientOrderModel?> GetById(int id) =>
        await _context.ClientOrders
            .Include(c => c.AppUser)
            .Include(c => c.ManufacturingOrders)
            .FirstOrDefaultAsync(c => c.Id == id);

    // Cria uma nova encomenda de cliente
    public async Task<ClientOrderModel> Create(ClientOrderModel entity)
    {
        // Adiciona a encomenda à base de dados
        _context.ClientOrders.Add(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();

        // Carrega a informação do cliente associado,
        // para que fique disponível no objeto devolvido
        await _context.Entry(entity)
            .Reference(c => c.AppUser)
            .LoadAsync();

        return entity;
    }

    // Atualiza uma encomenda existente
    public async Task Update(ClientOrderModel entity)
    {
        // Atualiza o registo
        _context.ClientOrders.Update(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();
    }

    // Remove uma encomenda
    public async Task Delete(int id)
    {
        // Procura a encomenda pelo ID
        var entity = await _context.ClientOrders.FindAsync(id);

        // Se existir, remove-a da base de dados
        if (entity != null)
        {
            _context.ClientOrders.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }

    // Verifica se existe uma encomenda com o ID indicado
    public async Task<bool> Exists(int id)
        => await _context.ClientOrders.AnyAsync(c => c.Id == id);
}