using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/Support
[Route("api/[controller]")]

// Apenas administradores podem gerir os suportes/skids
[Authorize(Roles = "admin")]
public class SupportController : ControllerBase
{
    // Repository responsável pelos suportes
    private readonly ISupportRepository _repo;

    // Service responsável pelo registo das ações no Audit Log
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente o repository e o service necessários
    public SupportController(ISupportRepository repo, IAuditService audit)
    {
        _repo  = repo;
        _audit = audit;
    }

    // GET /api/Support
    // Devolve uma lista paginada de suportes, permitindo aplicar filtros
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        // Número da página; por defeito começa na página 1
        [FromQuery] int page = 1,

        // Número de resultados apresentados por página
        [FromQuery] int pageSize = 25,

        // Pesquisa opcional, por exemplo pela tag RFID ou pelo tipo
        [FromQuery] string? search = null,

        // Filtro opcional pela linha de produção
        [FromQuery] int? productionLineId = null,

        // Filtro opcional para procurar suportes ocupados ou livres
        [FromQuery] bool? occupied = null)
    {
        // Pede ao repository os suportes com os filtros indicados
        var result = await _repo.GetPaged(
            page,
            pageSize,
            search,
            productionLineId,
            occupied
        );

        // Devolve HTTP 200 com os resultados
        return Ok(result);
    }

    // GET /api/Support/all
    // Devolve todos os suportes sem paginação
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todos os suportes
        var items = await _repo.GetAll();

        // Converte as entidades em DTOs antes de devolver
        return Ok(items.Select(s =>
            new SupportDTO(
                s.Id,
                s.ProductionLineId,
                s.RfidTag,
                s.Type
            )));
    }

    // GET /api/Support/{id}
    // Devolve um suporte específico através do seu ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura o suporte pelo ID
        var item = await _repo.GetById(id);

        // Se não existir, devolve 404
        if (item == null)
            return NotFound();

        // Devolve o suporte encontrado
        return Ok(new SupportDTO(
            item.Id,
            item.ProductionLineId,
            item.RfidTag,
            item.Type
        ));
    }

    // GET /api/Support/rfid/{tag}
    // Procura um suporte através da sua tag RFID
    [HttpGet("rfid/{tag}")]
    public async Task<IActionResult> GetByRfid(string tag)
    {
        // Procura o suporte pela tag RFID
        var item = await _repo.GetByRfidTag(tag);

        // Se não existir, devolve 404
        if (item == null)
            return NotFound();

        // Devolve o suporte encontrado
        return Ok(new SupportDTO(
            item.Id,
            item.ProductionLineId,
            item.RfidTag,
            item.Type
        ));
    }

    // POST /api/Support
    // Cria um novo suporte/skid
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSupportDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new SupportModel
        {
            ProductionLineId = dto.ProductionLineId,
            RfidTag = dto.RfidTag,
            Type = dto.Type
        };

        // Guarda o novo suporte na base de dados
        var created = await _repo.Create(entity);

        // Obtém o ID e o nome do utilizador autenticado
        var (userId, userName) = User.GetAuditUser();

        // Regista a criação no Audit Log
        await _audit.LogAsync(
            userId,
            userName,
            "created",
            "support",
            created.Id,
            $"{created.Type} – {created.RfidTag}"
        );

        // Devolve HTTP 201 com o suporte criado
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            new SupportDTO(
                created.Id,
                created.ProductionLineId,
                created.RfidTag,
                created.Type
            )
        );
    }

    // PUT /api/Support/{id}
    // Atualiza um suporte existente
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateSupportDTO dto)
    {
        // Procura o suporte pelo ID
        var entity = await _repo.GetById(id);

        // Se não existir, devolve 404
        if (entity == null)
            return NotFound();

        // Atualiza apenas os campos enviados
        if (dto.RfidTag != null)
            entity.RfidTag = dto.RfidTag;

        if (dto.Type != null)
            entity.Type = dto.Type;

        // Guarda as alterações
        await _repo.Update(entity);

        // Obtém os dados do utilizador para auditoria
        var (userId, userName) = User.GetAuditUser();

        // Regista a atualização no Audit Log
        await _audit.LogAsync(
            userId,
            userName,
            "updated",
            "support",
            entity.Id,
            $"{entity.Type} – {entity.RfidTag}"
        );

        // Devolve 204 porque a atualização correu bem,
        // mas não é necessário devolver conteúdo
        return NoContent();
    }

    // DELETE /api/Support/{id}
    // Remove um suporte
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura o suporte antes de o apagar
        var entity = await _repo.GetById(id);

        // Se não existir, devolve 404
        if (entity == null)
            return NotFound();

        // Remove o suporte da base de dados
        await _repo.Delete(id);

        // Obtém os dados do utilizador autenticado
        var (userId, userName) = User.GetAuditUser();

        // Regista a eliminação no Audit Log
        await _audit.LogAsync(
            userId,
            userName,
            "deleted",
            "support",
            id,
            $"{entity.Type} – {entity.RfidTag}"
        );

        return NoContent();
    }
}