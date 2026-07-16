using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller de API
[ApiController]

// Define a rota base: /api/analytics
[Route("api/analytics")]

// Apenas administradores e gestores podem consultar os indicadores
[Authorize(Roles = "admin,manager")]
public class AnalyticsController : ControllerBase
{
    // Service responsável por calcular e fornecer os indicadores
    private readonly IAnalyticsService _analyticsService;

    // O ASP.NET injeta automaticamente o service
    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    // GET /api/analytics/phase-durations
    // Devolve estatísticas sobre a duração das fases de fabrico
    [HttpGet("phase-durations")]
    public async Task<IActionResult> GetPhaseDurations()
    {
        // Obtém os dados através do service
        var result = await _analyticsService.GetPhaseDurationsAsync();

        // Devolve HTTP 200 com os resultados
        return Ok(result);
    }

    // GET /api/analytics/rework-rate
    // Devolve a taxa de rework das diferentes fases
    [HttpGet("rework-rate")]
    public async Task<IActionResult> GetReworkRate()
    {
        // Obtém os dados através do service
        var result = await _analyticsService.GetReworkRatesAsync();

        // Devolve HTTP 200 com os resultados
        return Ok(result);
    }

    // GET /api/analytics/throughput
    // Devolve indicadores de throughput (produtos concluídos)
    [HttpGet("throughput")]
    public async Task<IActionResult> GetThroughput()
    {
        // Obtém os dados através do service
        var result = await _analyticsService.GetThroughputAsync();

        // Devolve HTTP 200 com os resultados
        return Ok(result);
    }
}