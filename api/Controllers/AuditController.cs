using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/Audit
[Route("api/[controller]")]

// Apenas utilizadores com o papel de administrador podem aceder
[Authorize(Roles = "admin")]
public class AuditController : ControllerBase
{
    // Repository responsável por consultar os registos de auditoria
    private readonly IAuditRepository _repo;

    // O ASP.NET injeta automaticamente o repository
    public AuditController(IAuditRepository repo)
    {
        _repo = repo;
    }

    // GET /api/Audit?page=1&pageSize=25&entity=&userId=&action=
    // Devolve uma lista paginada de registos de auditoria,
    // permitindo aplicar filtros por entidade, utilizador e ação.
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? entity = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? action = null)
    {
        // Pede ao repository os registos que cumprem os filtros
        var result = await _repo.GetPagedAsync(page, pageSize, entity, userId, action);

        // Devolve HTTP 200 com os resultados
        return Ok(result);
    }

    // GET /api/Audit/users — lista de utilizadores distintos para o filtro
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        // Obtém todos os utilizadores que aparecem nos registos de auditoria
        var users = await _repo.GetDistinctUsersAsync();

        // Devolve apenas o ID e o nome de cada utilizador
        return Ok(users.Select(u => new { id = u.UserId, name = u.UserName }));
    }

    // GET /api/Audit/all
    // Devolve todos os registos de auditoria existentes
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
        => Ok(await _repo.GetAllAsync());

    // GET /api/Audit/entity/{entity}
    // Devolve apenas os registos associados à entidade indicada
    [HttpGet("entity/{entity}")]
    public async Task<IActionResult> GetByEntity(string entity)
        => Ok(await _repo.GetByEntityAsync(entity));

    // GET /api/Audit/user/{userId}
    // Devolve apenas os registos realizados pelo utilizador indicado
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(await _repo.GetByUserAsync(userId));
}