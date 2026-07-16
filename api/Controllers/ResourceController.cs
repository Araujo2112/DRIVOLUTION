using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/Resource
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    // Repository responsável pelos recursos da fábrica
    private readonly IResourceRepository _repo;

    // O ASP.NET injeta automaticamente o repository necessário
    public ResourceController(IResourceRepository repo) => _repo = repo;

    // GET /api/Resource
    // Devolve todos os recursos registados
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todos os recursos
        var items = await _repo.GetAll();

        // Converte para DTO e devolve o resultado
        return Ok(items.Select(r => new ResourceDTO(r.Id, r.IsHuman, r.Status)));
    }

    // GET /api/Resource/{id}
    // Procura um recurso pelo seu identificador
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura o recurso
        var item = await _repo.GetById(id);

        // Se não existir devolve 404
        if (item == null)
            return NotFound();

        // Devolve o recurso encontrado
        return Ok(new ResourceDTO(item.Id, item.IsHuman, item.Status));
    }

    // POST /api/Resource
    // Cria um novo recurso
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new ResourceModel
        {
            IsHuman = dto.IsHuman,

            // Se não for indicado um estado, assume Ativo por defeito
            Status = dto.Status ?? ActiveStatus.Active
        };

        // Guarda o recurso
        var created = await _repo.Create(entity);

        // Devolve 201 Created com o recurso criado
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            new ResourceDTO(created.Id, created.IsHuman, created.Status));
    }

    // PUT /api/Resource/{id}
    // Atualiza um recurso existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResourceDTO dto)
    {
        // Procura o recurso
        var entity = await _repo.GetById(id);

        // Se não existir devolve 404
        if (entity == null)
            return NotFound();

        // Atualiza apenas os campos enviados
        if (dto.IsHuman != null)
            entity.IsHuman = dto.IsHuman.Value;

        if (dto.Status != null)
            entity.Status = dto.Status;

        // Guarda as alterações
        await _repo.Update(entity);

        return NoContent();
    }

    // DELETE /api/Resource/{id}
    // Remove um recurso
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Verifica se o recurso existe
        if (!await _repo.Exists(id))
            return NotFound();

        // Remove o recurso
        await _repo.Delete(id);

        return NoContent();
    }
}