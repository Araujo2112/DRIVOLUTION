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

    // GET /api/Audit
    [HttpGet]
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