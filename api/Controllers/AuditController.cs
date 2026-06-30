using Drivolution.Repository.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class AuditController : ControllerBase
{
    private readonly IAuditRepository _repo;

    public AuditController(IAuditRepository repo)
    {
        _repo = repo;
    }

    // GET /api/Audit?page=1&pageSize=25&entity=&userId=&action=
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? entity = null,
        [FromQuery] int? userId = null,
        [FromQuery] string? action = null)
    {
        var result = await _repo.GetPagedAsync(page, pageSize, entity, userId, action);
        return Ok(result);
    }

    // GET /api/Audit/users — lista de utilizadores distintos para o filtro
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _repo.GetDistinctUsersAsync();
        return Ok(users.Select(u => new { id = u.UserId, name = u.UserName }));
    }

    // GET /api/Audit/all
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
        => Ok(await _repo.GetAllAsync());

    // GET /api/Audit/entity/{entity}
    [HttpGet("entity/{entity}")]
    public async Task<IActionResult> GetByEntity(string entity)
        => Ok(await _repo.GetByEntityAsync(entity));

    // GET /api/Audit/user/{userId}
    [HttpGet("user/{userId:int}")]
    public async Task<IActionResult> GetByUser(int userId)
        => Ok(await _repo.GetByUserAsync(userId));
}