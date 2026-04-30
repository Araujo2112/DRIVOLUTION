using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository;
using ApiTexPact.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeeController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetAll()
    {
        var employees = await _employeeService.GetAllEmployees();
        return Ok(employees);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id)
    {
        try
        {
            var employee = await _employeeService.GetEmployeeById(id);
            return Ok(employee);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto employee)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var createdEmployee = await _employeeService.CreateEmployee(employee);
        return CreatedAtAction(
            nameof(GetById), 
            new { id = createdEmployee.Id }, 
            createdEmployee);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] UpdateEmployeeDto employee)
    {
        try
        {
            var updated = await _employeeService.UpdateEmployee(id, employee);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _employeeService.DeleteEmployee(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("authenticate")]
    [AllowAnonymous]
    public async Task<ActionResult<EmployeeDto>> Authenticate([FromBody] LoginRequest request)
    {
        try
        {
            var employee = await _employeeService.AuthenticateEmployee(request.Username, request.Password);
            return Ok(employee);
        }
        catch (Exception)
        {
            return Unauthorized();
        }
    }
}