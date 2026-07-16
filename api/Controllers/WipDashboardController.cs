using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/production-lines
[Route("api/production-lines")]

// Administradores, gestores e operadores podem consultar o dashboard WIP
[Authorize(Roles = "admin,manager,operator")]
public class WipDashboardController : ControllerBase
{
    // Service responsável por obter a informação do dashboard WIP
    private readonly IWipDashboardService _service;

    // O ASP.NET injeta automaticamente o service necessário
    public WipDashboardController(IWipDashboardService service)
    {
        _service = service;
    }

    // GET /api/production-lines/wip
    // Devolve o estado atual dos produtos em produção (Work In Progress)
    [HttpGet("wip")]
    public async Task<IActionResult> GetWipDashboard()
    {
        // Obtém a informação do dashboard WIP
        var result = await _service.GetWipDashboardAsync();

        // Devolve 200 OK com os dados
        return Ok(result);
    }
}