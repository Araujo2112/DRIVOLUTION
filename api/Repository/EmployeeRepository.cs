using ApiTexPact.Data;
using ApiTexPact.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public interface IEmployeeRepository
{
    Task<IEnumerable<EmployeeModel>> GetAll();
    Task<EmployeeModel> GetById(int id);
    Task<EmployeeModel> GetByUsername(string username);
    Task<EmployeeModel> Create(EmployeeModel employee);
    Task Update(EmployeeModel employee);
    Task Delete(int id);
    Task<bool> Exists(int id);
}

public class EmployeeRepository : IEmployeeRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EmployeeModel>> GetAll()
    {
        return await _context.Employees.ToListAsync();
    }

    public async Task<EmployeeModel> GetById(int id)
    {
        return await _context.Employees.FindAsync(id)
               ?? throw new KeyNotFoundException($"Employee with ID {id} not found");
    }

    public async Task<EmployeeModel> GetByUsername(string username)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.Username == username)
               ?? throw new KeyNotFoundException($"Employee with username {username} not found");
    }

    public async Task<EmployeeModel> Create(EmployeeModel employee)
    {
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task Update(EmployeeModel employee)
    {
        _context.Entry(employee).State = EntityState.Modified;
        await _context.SaveChangesAsync();
    }
    
    public async Task Delete(int id)
    {
        var employee = await GetById(id);
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> Exists(int id)
    {
        return await _context.Employees.AnyAsync(e => e.Id == id);
    }
}