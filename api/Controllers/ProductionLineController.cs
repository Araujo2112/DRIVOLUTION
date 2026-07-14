using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class ProductionLineController : ControllerBase
{
    // Repository responsável por comunicar com a base de dados
    private readonly IProductionLineRepository _repo;
    // Service responsável por calcular previsões de ETA
    private readonly IEtaPredictionService     _etaService;
    // Service responsável por guardar registos de auditoria
    private readonly IAuditService             _audit;

    // O ASP.NET entrega automaticamente o repository e os services
    public ProductionLineController(IProductionLineRepository repo, IEtaPredictionService etaService, IAuditService audit)
    {
        _repo       = repo;
        _etaService = etaService;
        _audit      = audit;
    }

    // Devolve todas as linhas de produção
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Vai buscar todas as linhas de produção à base de dados
        var items = await _repo.GetAll();
        // Converte cada entidade para DTO e devolve 200 OK
        return Ok(items.Select(p => new ProductionLineDTO(p.Id, p.Name, p.Location, p.Status, p.Capacity)));
    }

    // Procura uma linha de produção pelo seu ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura a linha na base de dados
        var item = await _repo.GetById(id);
        // Se não existir, devolve 404
        if (item == null) return NotFound();
        // Converte a entidade para DTO e devolve 200 OK
        return Ok(new ProductionLineDTO(item.Id, item.Name, item.Location, item.Status, item.Capacity));
    }

    // Devolve as previsões de ETA da linha de produção
    [HttpGet("{id}/eta")]
    public async Task<IActionResult> GetEta(int id)
    {
        // Verifica se a linha de produção existe
        if (!await _repo.Exists(id)) return NotFound();
        // Pede ao service para calcular as previsões da linha
        var results = await _etaService.PredictForProductionLine(id);
        // Devolve 200 OK com os resultados
        return Ok(results);
    }

    // Cria uma nova linha de produção
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionLineDTO dto)
    {
        // Converte os dados recebidos no DTO para uma entidade da base de dados
        var entity  = new ProductionLineModel { Name = dto.Name, Location = dto.Location, Status = dto.Status, Capacity = dto.Capacity };
        // Guarda a nova linha de produção na base de dados
        var created = await _repo.Create(entity);

        // Obtém o ID e o nome do utilizador autenticado
        var (userId, userName) = User.GetAuditUser();
        // Guarda no histórico que o utilizador criou esta linha
        await _audit.LogAsync(userId, userName, "created", "production_line", created.Id, created.Name);

        // Devolve 201 Created com a linha criada
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new ProductionLineDTO(created.Id, created.Name, created.Location, created.Status, created.Capacity));
    }

    // Atualiza uma linha de produção existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductionLineDTO dto)
    {
        // Procura a linha de produção
        var entity = await _repo.GetById(id);
        // Se não existir, devolve 404
        if (entity == null) return NotFound();
        // Só altera o nome se tiver sido enviado um novo valor
        if (dto.Name     != null) entity.Name     = dto.Name;
        // Só altera a localização se tiver sido enviado um novo valor
        if (dto.Location != null) entity.Location = dto.Location;
        // Só altera o estado se tiver sido enviado um novo valor
        if (dto.Status   != null) entity.Status   = dto.Status;
        // Só altera a capacidade se tiver sido enviado um novo valor
        if (dto.Capacity != null) entity.Capacity = dto.Capacity;
        // Guarda as alterações na base de dados
        await _repo.Update(entity);

        // Obtém o utilizador que fez a alteração
        var (userId, userName) = User.GetAuditUser();
        // Guarda no histórico que a linha foi atualizada
        await _audit.LogAsync(userId, userName, "updated", "production_line", entity.Id, entity.Name);

        // Devolve 204 No Content
        return NoContent();
    }

    // Apaga uma linha de produção
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura a linha antes de a apagar
        var entity = await _repo.GetById(id);
        // Se não existir, devolve 404
        if (entity == null) return NotFound();
        // Apaga a linha da base de dados
        await _repo.Delete(id);

        // Obtém o utilizador que fez a eliminação
        var (userId, userName) = User.GetAuditUser();
        // Guarda no histórico que a linha foi apagada
        await _audit.LogAsync(userId, userName, "deleted", "production_line", id, entity.Name);

        // Devolve 204 No Content
        return NoContent();
    }
}
