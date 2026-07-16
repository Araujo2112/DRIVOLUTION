using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/Workstation
[Route("api/[controller]")]

/*
 Administradores, gestores e operadores podem consultar as workstations.
 Algumas operações específicas, como criar, editar e eliminar,
 estão limitadas apenas aos administradores.
*/
[Authorize(Roles = "admin,manager,operator")]
public class WorkstationController : ControllerBase
{
    // Repository responsável pelo acesso às workstations na base de dados
    private readonly IWorkstationRepository _repo;

    // Service responsável por registar as ações no Audit Log
    private readonly IAuditService _audit;

    // O ASP.NET injeta automaticamente o repository e o service necessários
    public WorkstationController(IWorkstationRepository repo, IAuditService audit)
    {
        _repo  = repo;
        _audit = audit;
    }

    // GET /api/Workstation
    // Devolve todas as workstations existentes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todas as workstations através do repository
        var items = await _repo.GetAll();

        // Converte cada WorkstationModel para WorkstationDTO
        return Ok(items.Select(w => ToDTO(w)));
    }

    // GET /api/Workstation/{id}
    // Devolve uma workstation específica através do seu ID
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura a workstation pelo ID
        var item = await _repo.GetById(id);

        // Se não existir, devolve 404 Not Found
        if (item == null) return NotFound();

        // Se existir, converte-a para DTO e devolve 200 OK
        return Ok(ToDTO(item));
    }

    // GET /api/Workstation/line/{productionLineId}
    // Devolve todas as workstations de uma determinada linha de produção
    [HttpGet("line/{productionLineId}")]
    public async Task<IActionResult> GetByProductionLine(int productionLineId)
    {
        // Obtém as workstations da linha indicada
        var items = await _repo.GetByProductionLine(productionLineId);

        // Converte os resultados para DTOs
        return Ok(items.Select(w => ToDTO(w)));
    }

    /// <summary>Workstations elegíveis para presença de operadores (human e hybrid).</summary>
    [HttpGet("human-eligible")]
    public async Task<IActionResult> GetHumanEligible()
    {
        // Obtém todas as workstations
        var items = await _repo.GetAll();

        // Mantém apenas as workstations cujo tipo permite presença humana
        var eligible = items.Where(w => WorkstationKind.HumanEligible.Contains(w.Kind));

        // Converte os resultados para DTOs
        return Ok(eligible.Select(w => ToDTO(w)));
    }

    // POST /api/Workstation
    // Cria uma nova workstation
    [HttpPost]

    // Apesar do controller permitir consulta a vários perfis,
    // apenas administradores podem criar workstations
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationDTO dto)
    {
        // Cria a entidade com os dados recebidos no DTO
        var entity = new WorkstationModel
        {
            ProductionLineId     = dto.ProductionLineId,
            Type                 = dto.Type,
            Kind                 = dto.Kind,
            ManufacturingPhaseId = dto.ManufacturingPhaseId,
        };

        // Guarda a nova workstation na base de dados
        var created = await _repo.Create(entity);

        // Volta a procurar a workstation para obter também
        // os dados relacionados, como a linha de produção e a fase
        var full = await _repo.GetById(created.Id);

        // Obtém os dados do utilizador autenticado para auditoria
        var (userId, userName) = User.GetAuditUser();

        // Cria uma descrição legível para o Audit Log
        var label =
            $"{full!.Type} " +
            $"(Linha {full.ProductionLine?.Name ?? full.ProductionLineId.ToString()})";

        // Regista a criação no Audit Log
        await _audit.LogAsync(
            userId,
            userName,
            "created",
            "workstation",
            created.Id,
            label
        );

        // Devolve 201 Created com a workstation criada
        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            ToDTO(full!)
        );
    }

    // PUT /api/Workstation/{id}
    // Atualiza uma workstation existente
    [HttpPut("{id}")]

    // Apenas administradores podem editar workstations
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateWorkstationDTO dto)
    {
        // Procura a workstation pelo ID
        var entity = await _repo.GetById(id);

        // Se não existir, devolve 404
        if (entity == null) return NotFound();

        // Atualiza apenas os campos que foram enviados no DTO
        if (dto.Type != null)
            entity.Type = dto.Type;

        if (dto.Kind != null)
            entity.Kind = dto.Kind;

        /*
         HasValue verifica se o nullable int recebeu um valor.
         Se recebeu, atualiza a fase de fabrico associada.
        */
        if (dto.ManufacturingPhaseId.HasValue)
            entity.ManufacturingPhaseId = dto.ManufacturingPhaseId;

        // Guarda as alterações na base de dados
        await _repo.Update(entity);

        // Obtém os dados do utilizador autenticado
        var (userId, userName) = User.GetAuditUser();

        // Regista a atualização no Audit Log
        await _audit.LogAsync(
            userId,
            userName,
            "updated",
            "workstation",
            entity.Id,
            entity.Type ?? $"Workstation {entity.Id}"
        );

        // Devolve 204 No Content
        return NoContent();
    }

    // DELETE /api/Workstation/{id}
    // Elimina uma workstation
    [HttpDelete("{id}")]

    // Apenas administradores podem eliminar workstations
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura a workstation antes de a eliminar
        var entity = await _repo.GetById(id);

        // Se não existir, devolve 404
        if (entity == null) return NotFound();

        // Elimina a workstation da base de dados
        await _repo.Delete(id);

        // Obtém os dados do utilizador autenticado
        var (userId, userName) = User.GetAuditUser();

        // Regista a eliminação no Audit Log
        await _audit.LogAsync(
            userId,
            userName,
            "deleted",
            "workstation",
            id,
            entity.Type ?? $"Workstation {id}"
        );

        // Devolve 204 No Content
        return NoContent();
    }

    // Método auxiliar que converte um WorkstationModel
    // num WorkstationDTO para enviar ao frontend
    private static WorkstationDTO ToDTO(WorkstationModel w) => new(
        w.Id,
        w.ProductionLineId,
        w.ProductionLine?.Name,
        w.Type,
        w.Kind,
        w.ManufacturingPhaseId,
        w.ManufacturingPhase?.Name
    );
}