using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/SupportedProduct
[Route("api/[controller]")]

// Administradores, gestores e operadores podem gerir as associações entre produtos e suportes
[Authorize(Roles = "admin,manager,operator")]
public class SupportedProductController : ControllerBase
{
    // Repository responsável pelas associações entre suportes e produtos
    private readonly ISupportedProductRepository _repo;

    // O ASP.NET injeta automaticamente o repository necessário
    public SupportedProductController(ISupportedProductRepository repo)
    {
        _repo = repo;
    }

    // GET /api/SupportedProduct/support/{supportId}/current
    // Devolve a associação atualmente ativa de um suporte
    [HttpGet("support/{supportId}/current")]
    public async Task<IActionResult> GetCurrent(int supportId)
    {
        // Procura a associação ativa do suporte
        var item = await _repo.GetCurrentBySupport(supportId);

        // Se não existir devolve 404
        if (item == null)
            return NotFound();

        // Devolve a associação encontrada
        return Ok(ToDTO(item));
    }

    // GET /api/SupportedProduct/support/{supportId}
    // Devolve o histórico de associações de um suporte
    [HttpGet("support/{supportId}")]
    public async Task<IActionResult> GetBySupport(int supportId)
    {
        // Obtém todas as associações do suporte
        var items = await _repo.GetBySupport(supportId);

        // Converte para DTO e devolve o resultado
        return Ok(items.Select(ToDTO));
    }

    // Associar produto a suporte — fecha o anterior automaticamente
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportedProductDTO dto)
    {
        // Verifica se o suporte já tem uma associação ativa
        var current = await _repo.GetCurrentBySupport(dto.SupportId);

        // Se existir, fecha essa associação
        if (current != null)
        {
            current.DatetimeEnd = DateTime.UtcNow;
            await _repo.Update(current);
        }

        // Cria uma nova associação entre o suporte e o produto
        var entity = new SupportedProductModel
        {
            SupportId = dto.SupportId,
            ProductId = dto.ProductId,
            DatetimeIni = DateTime.UtcNow,
        };

        // Guarda a nova associação
        await _repo.Create(entity);

        // Obtém novamente a associação criada com todos os dados relacionados
        var full = await _repo.GetCurrentBySupport(dto.SupportId);

        // Devolve 201 Created com a nova associação
        return CreatedAtAction(
            nameof(GetCurrent),
            new { supportId = dto.SupportId },
            ToDTO(full!)
        );
    }

    // Libertar suporte — fecha a associação ativa
    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(int id)
    {
        // Procura a associação pelo ID
        var item = await _repo.GetById(id);

        // Se não existir devolve 404
        if (item == null)
            return NotFound();

        // Se já estiver fechada devolve erro
        if (item.DatetimeEnd != null)
            return BadRequest("Esta associação já está fechada.");

        // Fecha a associação registando a data de fim
        item.DatetimeEnd = DateTime.UtcNow;

        // Guarda as alterações
        await _repo.Update(item);

        return NoContent();
    }

    // Converte uma entidade SupportedProductModel para o respetivo DTO
    private static SupportedProductDTO ToDTO(SupportedProductModel sp) => new(
        sp.Id,
        sp.SupportId,
        sp.ProductId,
        sp.Product?.SerialNumber,
        sp.Product?.CarModel?.Name,
        sp.DatetimeIni,
        sp.DatetimeEnd
    );
}