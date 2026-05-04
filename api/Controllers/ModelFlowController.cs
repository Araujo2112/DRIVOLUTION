using ApiTexPact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/models")]
public class ModelFlowController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ModelFlowController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}/flow")]
    public async Task<IActionResult> GetModelFlow(int id)
    {
        var modelExists = await _context.DrivolutionModels.AnyAsync(m => m.Id == id);

        if (!modelExists)
            return NotFound("Model does not exist.");

        var flow = await _context.PhaseSequences
            .Where(ps => ps.ModelId == id)
            .Include(ps => ps.ManufacturingPhase)
            .OrderBy(ps => ps.Order)
            .Select(ps => new
            {
                order = ps.Order,
                manufacturingPhaseId = ps.ManufacturingPhaseId,
                phaseName = ps.ManufacturingPhase != null ? ps.ManufacturingPhase.Name : null,
                estimatedDuration = ps.ManufacturingPhase != null ? ps.ManufacturingPhase.EstimatedDuration : null
            })
            .ToListAsync();

        return Ok(flow);
    }

    [HttpGet("{id}/simulate")]
    public async Task<IActionResult> SimulateModelProduction(int id)
    {
        var model = await _context.DrivolutionModels.FindAsync(id);

        if (model == null)
            return NotFound("Model does not exist.");

        var steps = await _context.PhaseSequences
            .Where(ps => ps.ModelId == id)
            .Include(ps => ps.ManufacturingPhase)
            .OrderBy(ps => ps.Order)
            .Select(ps => new
            {
                order = ps.Order,
                manufacturingPhaseId = ps.ManufacturingPhaseId,
                phaseName = ps.ManufacturingPhase != null ? ps.ManufacturingPhase.Name : null,
                estimatedDuration = ps.ManufacturingPhase != null ? ps.ManufacturingPhase.EstimatedDuration : 0
            })
            .ToListAsync();

        if (!steps.Any())
            return BadRequest("This model has no production flow defined.");

        var totalDuration = steps.Sum(s => s.estimatedDuration);

        var bottleneck = steps
            .OrderByDescending(s => s.estimatedDuration)
            .First();

        return Ok(new
        {
            modelId = model.Id,
            modelName = model.Name,
            totalEstimatedDuration = totalDuration,
            bottleneck = new
            {
                phaseName = bottleneck.phaseName,
                duration = bottleneck.estimatedDuration
            },
            steps
        });
    }
}