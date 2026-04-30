using System.Security.Cryptography;
using System.Text;
using ApiTexPact.DTO;
using ApiTexPact.Repository;
using ApiTexPact.Models;

namespace ApiTexPact.Services;

public class AuthResponse
{
    public string Token { get; set; }
    public EmployeeDto Employee { get; set; }
}

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeDto>> GetAllEmployees();
    Task<EmployeeDto> GetEmployeeById(int id);
    Task<EmployeeDto> CreateEmployee(CreateEmployeeDto employee);
    Task<EmployeeDto> UpdateEmployee(int id, UpdateEmployeeDto employee);
    Task DeleteEmployee(int id);
    Task<AuthResponse> AuthenticateEmployee(string username, string password);
}

public class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _repository;
    private readonly IAuthService _authService;

    public EmployeeService(IEmployeeRepository repository, IAuthService authService)
    {
        _repository = repository;
        _authService = authService;
    }

    public async Task<IEnumerable<EmployeeDto>> GetAllEmployees()
    {
        var employees = await _repository.GetAll();
        return employees.Select(ToDto);
    }

    public async Task<EmployeeDto> GetEmployeeById(int id)
    {
        var employee = await _repository.GetById(id);
        return ToDto(employee);
    }

    public async Task<EmployeeDto> CreateEmployee(CreateEmployeeDto dto)
    {
        var employee = new EmployeeModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Username = dto.Username,
            Password = HashPassword(dto.Password),
            WatchId = dto.WatchId
        };

        var created = await _repository.Create(employee);
        return ToDto(created);
    }

    public async Task<EmployeeDto> UpdateEmployee(int id, UpdateEmployeeDto dto)
    {
        var existing = await _repository.GetById(id);
        
        existing.FirstName = dto.FirstName;
        existing.LastName = dto.LastName;
        existing.Username = dto.Username;
        existing.WatchId = dto.WatchId;
        
        if (!string.IsNullOrEmpty(dto.Password))
        {
            existing.Password = HashPassword(dto.Password);
        }

        await _repository.Update(existing);
        return ToDto(existing);
    }

    public async Task DeleteEmployee(int id)
    {
        await _repository.Delete(id);
    }

    public async Task<AuthResponse> AuthenticateEmployee(string username, string password)
    {
        var employee = await _repository.GetByUsername(username);
        var hashedPassword = HashPassword(password);
        
        if (employee.Password != hashedPassword)
        {
            throw new UnauthorizedAccessException("Invalid credentials");
        }

        var employeeDto = ToDto(employee);
        var token = await _authService.GenerateToken(employeeDto);
        
        return new AuthResponse 
        { 
            Token = token,
            Employee = employeeDto
        };
    }

    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private static EmployeeDto ToDto(EmployeeModel employee)
    {
        return new EmployeeDto
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Username = employee.Username,
            WatchId = employee.WatchId
        };
    }
}