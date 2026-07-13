using Drivolution.DTO;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,operator")]
public class QualityCheckController : ControllerBase
{
    // Service onde está toda a lógica dos controlos de qualidade
    private readonly IQualityCheckService _service;

    // O ASP.NET entrega automaticamente o service ao criar o controller
    public QualityCheckController(IQualityCheckService service)
    {
        _service = service;
    }

    // Devolve todos os controlos de qualidade de um produto
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var items = await _service.GetByProduct(productId);
        return Ok(items);
    }

    // Procura um controlo de qualidade pelo seu ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Pede ao service o controlo de qualidade
        var item = await _service.GetById(id);
        // Se não existir devolve 404
        if (item == null) return NotFound(new { message = "Quality Check não encontrado." });
        // Se existir devolve 200 OK
        return Ok(item);
    }

    // Cria um novo controlo de qualidade
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateQualityCheckDTO dto)
    {
        try 
        {
            // Pede ao service para criar o controlo de qualidade
            var result = await _service.Create(dto);
            // Devolve 201 Created e o objeto criado
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        // Se algum elemento necessário não existir
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        // Qualquer outro erro durante a criação
        catch (Exception ex)
        {
            return BadRequest(new { message = "Erro ao processar controlo de qualidade.", details = ex.Message });
        }
    }
}