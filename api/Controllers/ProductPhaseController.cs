using Drivolution.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Drivolution.Services.Interface;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,operator")]
public class ProductPhaseController : ControllerBase //a classe herda funcionalidades próprias de um controller
{
    // Service onde está toda a lógica das fases dos produtos
    private readonly IProductPhaseService _service; //cria uma variável chamada _service

    // O ASP.NET entrega automaticamente o service ao criar o controller
    public ProductPhaseController(IProductPhaseService service)
    {
        _service = service;
    }

    // Devolve todas as fases de um produto
    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        // Pede ao service as fases do produto
        var items = await _service.GetByProduct(productId);
        // Devolve 200 OK com a lista
        return Ok(items);
    }

    // Devolve a fase atual do produto
    [HttpGet("product/{productId}/current")]
    public async Task<IActionResult> GetCurrent(int productId)
    {
        // Procura a fase atual
        var item = await _service.GetCurrent(productId);
        // Se não existir devolve 404
        if (item == null) return NotFound();
        // Caso exista devolve 200 OK
        return Ok(item);
    }

    // Cria uma nova fase para um produto
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductPhaseDTO dto)
    {
        try
        {
            // Pede ao service para criar a fase
            var created = await _service.Create(dto);
            // Devolve 201 Created e o objeto criado
            return CreatedAtAction(nameof(GetCurrent), new { productId = created.ProductId }, created);
        }
        // Se existir algum conflito
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        // Se não encontrar algum elemento
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // Fecha uma fase existente
    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] CloseProductPhaseDTO dto)
    {
        try
        {
            // Pede ao service para fechar a fase
            await _service.Close(id, dto);
            // Operação concluída com sucesso
            return NoContent();
        }
        // Se a fase não existir
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
}