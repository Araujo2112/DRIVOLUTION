using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller de API
[ApiController]

// Define a rota base: /api/Alert
[Route("api/[controller]")]

/*
 Apenas utilizadores com uma destas roles podem aceder:
 admin, manager ou operator
*/
[Authorize(Roles = "admin,manager,operator")]
public class AlertController : ControllerBase
{
    // Service responsável pela lógica dos alertas
    private readonly IAlertService _alertService;

    // O ASP.NET injeta automaticamente o service
    public AlertController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    // GET /api/Alert
    // Devolve uma lista paginada de alertas
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        // Número da página; por defeito começa na página 1
        [FromQuery] int page = 1,

        // Quantidade de alertas por página
        [FromQuery] int pageSize = 25,

        // Filtro opcional pelo tipo de alerta
        [FromQuery] string? type = null,

        // Filtro opcional pelo estado do alerta
        [FromQuery] string? status = null)
    {
        // Chama o service e envia os filtros recebidos
        var result = await _alertService.GetPagedAsync(
            page,
            pageSize,
            type,
            status
        );

        // Devolve HTTP 200 com o resultado
        return Ok(result);
    }

    // GET /api/Alert/all
    // Devolve todos os alertas sem paginação
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
        => Ok(await _alertService.GetAllAsync());

    // GET /api/Alert/open
    // Devolve apenas os alertas que ainda estão abertos
    [HttpGet("open")]
    public async Task<IActionResult> GetOpen()
        => Ok(await _alertService.GetOpenAsync());

    // PUT /api/Alert/{id}/acknowledge
    // Marca um alerta como reconhecido
    [HttpPut("{id}/acknowledge")]
    public async Task<IActionResult> Acknowledge(int id)
    {
        // Pede ao service para reconhecer o alerta
        var result = await _alertService.AcknowledgeAsync(id);

        // Se o alerta não existir, devolve 404
        if (result == null)
            return NotFound();

        // Caso exista, devolve 200 com o alerta atualizado
        return Ok(result);
    }

    // PUT /api/Alert/{id}/resolve
    // Marca um alerta como resolvido
    [HttpPut("{id}/resolve")]
    public async Task<IActionResult> Resolve(int id)
    {
        // Pede ao service para resolver o alerta
        var result = await _alertService.ResolveAsync(id);

        // Se o alerta não existir, devolve 404
        if (result == null)
            return NotFound();

        // Caso exista, devolve 200 com o alerta atualizado
        return Ok(result);
    }
}