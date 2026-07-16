using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/production-lines
[Route("api/production-lines")]

// Administradores, gestores e operadores podem consultar o estado da linha de produção
[Authorize(Roles = "admin,manager,operator")]
public class ProductionLineStatusController : ControllerBase
{
    // Service responsável por obter o estado das linhas de produção
    private readonly IProductionLineStatusService _service;

    // O ASP.NET injeta automaticamente o service necessário
    public ProductionLineStatusController(IProductionLineStatusService service)
    {
        _service = service;
    }

    // GET /api/production-lines/status
    // Devolve o estado atual de todas as linhas de produção
    [HttpGet("status")]
    public async Task<IActionResult> GetProductionLineStatus()
    {
        // Obtém a informação da linha de produção através do service
        var status = await _service.GetProductionLineStatusAsync();

        // Devolve 200 OK com o estado atual
        return Ok(status);
    }
}