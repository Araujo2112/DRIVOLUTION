using ApiTexPact.Data;
using ApiTexPact.DTO;
using ApiTexPact.Services.ArrowFlightClient.Interfaces;
using ApiTexPact.Services.Prediction.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers.Prediction;

[ApiController]
[Route("api/[controller]")]
public class PredictionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IPredictionService _predictionService;
    private readonly IQuicPredictionClientService _quic;

    public PredictionController(ApplicationDbContext context,
        IQuicPredictionClientService quic,
        IPredictionService predictionService)
    {
        _context = context;
        _quic = quic;
        _predictionService = predictionService;
    }


    [HttpPost("train")]
    public async Task<IActionResult> Train()
    {
        try
        {
            var result = await _predictionService.TrainAsync();
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("predict")]
    public async Task<ActionResult<float[]>> Predict([FromBody] PredictRequestDto req)
    {
        try
        {
            var preds = await _predictionService.PredictAsync(req.Features);
            return Ok(preds);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Erro interno: " + ex.Message });
        }
    }
}

/*
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
*/