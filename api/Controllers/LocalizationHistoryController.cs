using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/LocalizationHistory
[Route("api/[controller]")]

// Apenas administradores, gestores e operadores podem consultar o histórico de localização
[Authorize(Roles = "admin,manager,operator")]
public class LocalizationHistoryController : ControllerBase
{
    // Service responsável pelo histórico de localização dos suportes (skids)
    private readonly ILocalizationHistoryService _service;

    // O ASP.NET injeta automaticamente o service necessário
    public LocalizationHistoryController(ILocalizationHistoryService service)
    {
        _service = service;
    }

    // GET /api/LocalizationHistory/support/{supportId}
    // Devolve todo o histórico de localizações de um suporte
    [HttpGet("support/{supportId}")]
    public async Task<IActionResult> GetBySupport(int supportId)
    {
        var items = await _service.GetBySupport(supportId);
        return Ok(items);
    }

    // GET /api/LocalizationHistory/support/{supportId}/current
    // Devolve a localização atual de um suporte
    [HttpGet("support/{supportId}/current")]
    public async Task<IActionResult> GetCurrent(int supportId)
    {
        var item = await _service.GetCurrent(supportId);

        if (item == null)
            return NotFound();

        return Ok(item);
    }

    // POST /api/LocalizationHistory
    // Cria um novo registo de localização para um suporte
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateLocalizationHistoryDTO dto)
    {
        // Guarda o novo registo no histórico
        var created = await _service.Create(dto);

        // Devolve o registo criado
        return CreatedAtAction(
            nameof(GetCurrent),
            new { supportId = created.SupportId },
            created);
    }
}