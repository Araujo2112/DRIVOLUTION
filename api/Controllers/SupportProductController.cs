using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupportedProductController : ControllerBase
{
    private readonly ISupportedProductRepository _repo;

    public SupportedProductController(ISupportedProductRepository repo)
    {
        _repo = repo;
    }

    [HttpGet("support/{supportId}/current")]
    public async Task<IActionResult> GetCurrent(int supportId)
    {
        var item = await _repo.GetCurrentBySupport(supportId);
        if (item == null) return NotFound();
        return Ok(ToDTO(item));
    }

    [HttpGet("support/{supportId}")]
    public async Task<IActionResult> GetBySupport(int supportId)
    {
        var items = await _repo.GetBySupport(supportId);
        return Ok(items.Select(ToDTO));
    }

    // Associar produto a suporte — fecha o anterior automaticamente
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportedProductDTO dto)
    {
        // Fechar associação ativa anterior
        var current = await _repo.GetCurrentBySupport(dto.SupportId);
        if (current != null)
        {
            current.DatetimeEnd = DateTime.UtcNow;
            await _repo.Update(current);
        }

        var entity = new SupportedProductModel
        {
            SupportId = dto.SupportId,
            ProductId = dto.ProductId,
            DatetimeIni = DateTime.UtcNow,
        };

        await _repo.Create(entity);
        var full = await _repo.GetCurrentBySupport(dto.SupportId);
        return CreatedAtAction(nameof(GetCurrent), new { supportId = dto.SupportId }, ToDTO(full!));
    }

    // Libertar suporte — fecha a associação ativa
    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        if (item.DatetimeEnd != null) return BadRequest("Esta associação já está fechada.");

        item.DatetimeEnd = DateTime.UtcNow;
        await _repo.Update(item);
        return NoContent();
    }

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