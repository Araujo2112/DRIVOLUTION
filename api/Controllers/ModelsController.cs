using ApiTexPact.Data;
using ApiTexPact.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/models")]
public class ModelsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ModelsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET ALL MODELS
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DrivolutionModel>>> GetModels()
    {
        return await _context.DrivolutionModels.ToListAsync();
    }

    // GET MODEL BY ID
    [HttpGet("{id}")]
    public async Task<ActionResult<DrivolutionModel>> GetModel(int id)
    {
        var model = await _context.DrivolutionModels.FindAsync(id);

        if (model == null)
            return NotFound("Model not found.");

        return model;
    }

    // CREATE MODEL
    [HttpPost]
    public async Task<ActionResult<DrivolutionModel>> CreateModel(DrivolutionModel model)
    {
        _context.DrivolutionModels.Add(model);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetModel), new { id = model.Id }, model);
    }
}