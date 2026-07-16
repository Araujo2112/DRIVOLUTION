using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/WorkstationStatus
[Route("api/[controller]")]
public class WorkstationStatusController : ControllerBase
{
    // Repository responsável pelos estados das workstations
    private readonly IWorkstationStatusRepository _repo;

    // O ASP.NET injeta automaticamente o repository necessário
    public WorkstationStatusController(IWorkstationStatusRepository repo) => _repo = repo;

    // GET /api/WorkstationStatus/workstation/{workstationId}
    // Devolve todo o histórico de estados de uma workstation
    [HttpGet("workstation/{workstationId}")]
    public async Task<IActionResult> GetByWorkstation(int workstationId)
    {
        // Obtém todos os estados associados à workstation indicada
        var items = await _repo.GetByWorkstation(workstationId);

        // Converte cada entidade para DTO antes de devolver
        return Ok(items.Select(ws =>
            new WorkstationStatusDTO(
                ws.Id,
                ws.WorkstationId,
                ws.Status,
                ws.Timestamp
            )));
    }

    // GET /api/WorkstationStatus/workstation/{workstationId}/latest
    // Devolve o estado mais recente de uma workstation
    [HttpGet("workstation/{workstationId}/latest")]
    public async Task<IActionResult> GetLatest(int workstationId)
    {
        // Procura o último estado registado para a workstation
        var item = await _repo.GetLatestByWorkstation(workstationId);

        // Se não existir nenhum registo, devolve 404
        if (item == null)
            return NotFound();

        // Devolve o estado mais recente
        return Ok(new WorkstationStatusDTO(
            item.Id,
            item.WorkstationId,
            item.Status,
            item.Timestamp
        ));
    }

    // POST /api/WorkstationStatus
    // Cria um novo registo de estado para uma workstation
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateWorkstationStatusDTO dto)
    {
        // Cria a entidade a partir dos dados recebidos
        var entity = new WorkstationStatusModel
        {
            WorkstationId = dto.WorkstationId,

            // Se não for enviado um estado,
            // assume "Functional" por defeito
            Status = dto.Status ?? WorkstationStatusConstants.Functional,

            // Guarda a data e hora atuais em UTC
            Timestamp = DateTime.UtcNow
        };

        // Guarda o novo estado na base de dados
        var created = await _repo.Create(entity);

        // Devolve 201 Created com o estado criado
        return CreatedAtAction(
            nameof(GetLatest),
            new { workstationId = created.WorkstationId },
            new WorkstationStatusDTO(
                created.Id,
                created.WorkstationId,
                created.Status,
                created.Timestamp
            )
        );
    }
}