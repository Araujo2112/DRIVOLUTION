using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/WorkstationAllocation
[Route("api/[controller]")]
public class WorkstationAllocationController : ControllerBase
{
    // Repository responsável pelas alocações de recursos às workstations
    private readonly IWorkstationAllocationRepository _repo;

    // O ASP.NET injeta automaticamente o repository necessário
    public WorkstationAllocationController(IWorkstationAllocationRepository repo)
    {
        _repo = repo;
    }

    // GET /api/WorkstationAllocation
    // Devolve todas as alocações existentes
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        // Obtém todas as alocações
        var items = await _repo.GetAll();

        // Devolve 200 OK com os resultados
        return Ok(items);
    }

    // GET /api/WorkstationAllocation/workstation/{workstationId}
    // Devolve todas as alocações de uma workstation
    [HttpGet("workstation/{workstationId}")]
    public async Task<IActionResult> GetByWorkstation(int workstationId)
    {
        // Obtém as alocações da workstation indicada
        var items = await _repo.GetByWorkstation(workstationId);

        // Devolve os resultados
        return Ok(items);
    }

    // GET /api/WorkstationAllocation/{id}
    // Procura uma alocação pelo seu identificador
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Procura a alocação
        var item = await _repo.GetById(id);

        // Se não existir devolve 404
        if (item == null)
            return NotFound();

        // Devolve a alocação encontrada
        return Ok(item);
    }

    // POST /api/WorkstationAllocation
    // Cria uma nova alocação de um recurso a uma workstation
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationAllocationDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new WorkstationAllocationModel
        {
            ResourceId = dto.ResourceId,
            WorkstationId = dto.WorkstationId,

            // Caso não seja fornecida uma data de início,
            // utiliza a data e hora atuais
            StartDate = dto.StartDate == default
                ? DateTime.UtcNow
                : dto.StartDate,

            // Caso não seja indicado um estado,
            // assume que a alocação está ativa
            Status = dto.Status ?? ActiveStatus.Active
        };

        // Guarda a nova alocação
        var created = await _repo.Create(entity);

        // Converte a entidade para DTO
        var resultDTO = new WorkstationAllocationDTO(
            created.Id,
            created.ResourceId,
            created.Resource?.IsHuman ?? false,
            created.WorkstationId,
            created.Status,
            created.StartDate,
            created.EndDate
        );

        // Devolve 201 Created com a alocação criada
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, resultDTO);
    }

    // PUT /api/WorkstationAllocation/{id}/finish
    // Termina uma alocação ativa
    [HttpPut("{id}/finish")]
    public async Task<IActionResult> FinishAllocation(int id)
    {
        // Procura a alocação
        var entity = await _repo.GetById(id);

        // Se não existir devolve 404
        if (entity == null)
            return NotFound();

        // Quando a alocação termina, mudamos o status e registamos a data de fim
        entity.Status = ActiveStatus.Inactive;
        entity.EndDate = DateTime.UtcNow;

        // Guarda as alterações
        await _repo.Update(entity);

        return NoContent();
    }

    // DELETE /api/WorkstationAllocation/{id}
    // Remove uma alocação
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Procura a alocação
        var entity = await _repo.GetById(id);

        // Se não existir devolve 404
        if (entity == null)
            return NotFound();

        // Remove a alocação
        await _repo.Delete(id);

        return NoContent();
    }
}