using Drivolution.Data;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

// Repository responsável por gerir as alocações de recursos às workstations
public class WorkstationAllocationRepository : IWorkstationAllocationRepository
{
    // Contexto da base de dados
    private readonly ApplicationDbContext _context;

    // O ASP.NET injeta automaticamente o DbContext
    public WorkstationAllocationRepository(ApplicationDbContext context) => _context = context;

    // Devolve todas as alocações existentes,
    // incluindo o recurso e a workstation associados
    public async Task<IEnumerable<WorkstationAllocationModel>> GetAll() =>
        await _context.WorkstationAllocations
            .Include(wa => wa.Resource)
            .Include(wa => wa.Workstation)
            .ToListAsync();

    // Devolve todas as alocações associadas a uma determinada workstation
    public async Task<IEnumerable<WorkstationAllocationModel>> GetByWorkstation(int workstationId) =>
        await _context.WorkstationAllocations
            .Where(wa => wa.WorkstationId == workstationId)
            .Include(wa => wa.Resource)
            .ToListAsync();

    // Procura uma alocação pelo seu ID
    public async Task<WorkstationAllocationModel?> GetById(int id) =>
        await _context.WorkstationAllocations.FindAsync(id);

    // Cria uma nova alocação entre um recurso e uma workstation
    public async Task<WorkstationAllocationModel> Create(WorkstationAllocationModel entity)
    {
        // Adiciona a nova alocação à base de dados
        _context.WorkstationAllocations.Add(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();

        return entity;
    }

    // Atualiza uma alocação existente
    public async Task Update(WorkstationAllocationModel entity)
    {
        // Marca a entidade como alterada
        _context.WorkstationAllocations.Update(entity);

        // Guarda as alterações
        await _context.SaveChangesAsync();
    }

    // Remove uma alocação
    public async Task Delete(int id)
    {
        // Procura a alocação pelo ID
        var entity = await _context.WorkstationAllocations.FindAsync(id);

        // Só remove se a alocação existir
        if (entity != null)
        {
            _context.WorkstationAllocations.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}