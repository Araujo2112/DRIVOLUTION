using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/Material
[Route("api/[controller]")]

// Apenas administradores podem gerir os materiais
[Authorize(Roles = "admin")]
public class MaterialController : ControllerBase
{
    // Repository responsável pelos materiais
    private readonly IMaterialRepository _repo;

    // O ASP.NET injeta automaticamente o repository necessário
    public MaterialController(IMaterialRepository repo) => _repo = repo;

    // GET /api/Material
    // Devolve todos os materiais registados
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todos os materiais da base de dados
        var items = await _repo.GetAll();

        // Converte as entidades em DTOs antes de devolver ao cliente
        return Ok(items.Select(m => new MaterialDTO(
            m.Id,
            m.Item,
            m.PartNumber)));
    }

    // GET /api/Material/{id}
    // Devolve um material específico
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura o material pelo ID
        var item = await _repo.GetById(id);

        if (item == null)
            return NotFound();

        // Devolve o material encontrado
        return Ok(new MaterialDTO(
            item.Id,
            item.Item,
            item.PartNumber));
    }

    // POST /api/Material
    // Cria um novo material
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMaterialDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new MaterialModel
        {
            Item = dto.Item,
            PartNumber = dto.PartNumber
        };

        // Guarda o novo material na base de dados
        var created = await _repo.Create(entity);

        // Devolve o material criado
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            new MaterialDTO(
                created.Id,
                created.Item,
                created.PartNumber));
    }

    // PUT /api/Material/{id}
    // Atualiza um material existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateMaterialDTO dto)
    {
        // Procura o material
        var entity = await _repo.GetById(id);

        if (entity == null)
            return NotFound();

        // Atualiza apenas os campos enviados
        if (dto.Item != null)
            entity.Item = dto.Item;

        if (dto.PartNumber != null)
            entity.PartNumber = dto.PartNumber;

        // Guarda as alterações
        await _repo.Update(entity);

        return NoContent();
    }

    // DELETE /api/Material/{id}
    // Remove um material
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Verifica se o material existe
        if (!await _repo.Exists(id))
            return NotFound();

        // Remove o material da base de dados
        await _repo.Delete(id);

        return NoContent();
    }
}