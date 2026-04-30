using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class ContainerRepository : IContainerRepository
{
    private readonly ApplicationDbContext _context;

    public ContainerRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<List<ContainerModel>> GetAllContainersAsync()
    {
        return await _context.Containers
            .ToListAsync();
    }

    public async Task<ContainerModel?> GetContainerById(int containerId)
    {
        return await _context.Containers
            .FirstOrDefaultAsync(c => c.ContainerId == containerId);
        
    }
    
    
    
    public async Task<ContainerModel?> GetContainerByCodeAsync(string containerCode)
    {
        return await _context.Containers
            .FirstOrDefaultAsync(c => c.ContainerCode == containerCode); 
    }

    public async Task AddContainerAsync(ContainerModel container)
    {
        _context.Containers.Add(container);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateContainerAsync(ContainerModel container)
    {
        _context.Containers.Update(container);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteContainerAsync(int containerId)
    {
        var container = await GetContainerById(containerId);
        if (container == null) return;

        _context.Containers.Remove(container);
        await _context.SaveChangesAsync();
    }
}