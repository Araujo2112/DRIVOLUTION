using ApiTexPact.Data;
using ApiTexPact.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/production-lines")]
public class ProductionLinesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductionLinesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductionLineModel>>> GetProductionLines()
    {
        return await _context.ProductionLines.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductionLineModel>> GetProductionLine(int id)
    {
        var productionLine = await _context.ProductionLines.FindAsync(id);

        if (productionLine == null)
            return NotFound();

        return productionLine;
    }
}