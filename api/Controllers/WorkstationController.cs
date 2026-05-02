using ApiTexPact.Data;
using ApiTexPact.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/workstations")]
public class WorkstationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WorkstationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<WorkstationModel>>> GetWorkstations()
    {
        return await _context.Workstations
            .Include(w => w.ProductionLine)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<WorkstationModel>> GetWorkstation(int id)
    {
        var workstation = await _context.Workstations
            .Include(w => w.ProductionLine)
            .FirstOrDefaultAsync(w => w.Id == id);

        if (workstation == null)
            return NotFound();

        return workstation;
    }

    [HttpPost]
    public async Task<ActionResult<WorkstationModel>> CreateWorkstation(WorkstationModel workstation)
    {
        var productionLineExists = await _context.ProductionLines
            .AnyAsync(pl => pl.Id == workstation.ProductionLineId);

        if (!productionLineExists)
            return BadRequest("Production line does not exist.");

        _context.Workstations.Add(workstation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetWorkstation),
            new { id = workstation.Id },
            workstation
        );
    }
}