using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ResourceController : ControllerBase
{
    private readonly IResourceRepository _repo;
    public ResourceController(IResourceRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(r => new ResourceDTO(r.Id, r.IsHuman, r.Status)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ResourceDTO(item.Id, item.IsHuman, item.Status));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateResourceDTO dto)
    {
        var entity = new ResourceModel 
        { 
            IsHuman = dto.IsHuman, 
            Status = dto.Status ?? EntityStatus.Active // Se vier vazio, assume Ativo
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ResourceDTO(created.Id, created.IsHuman, created.Status));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateResourceDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        
        if (dto.IsHuman != null) entity.IsHuman = dto.IsHuman.Value;
        if (dto.Status != null) entity.Status = dto.Status;
        
        await _repo.Update(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _repo.Exists(id)) return NotFound();
        await _repo.Delete(id);
        return NoContent();
    }
}