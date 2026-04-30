using ApiTexPact.DTO;
using ApiTexPact.Services.Interface.ManufacturingOrder;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingOrderController : ControllerBase
{
    private readonly IManufacturingOrderService _service;

    public ManufacturingOrderController(IManufacturingOrderService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturingOrderDTO>>> GetAll()
    {
        var orders = await _service.GetAllOrders();
        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ManufacturingOrderDTO>> GetById(int id)
    {
        var order = await _service.GetOrderById(id);
        if (order == null)
            return NotFound();

        return Ok(order);
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturingOrderDTO>> Create([FromBody] CreateManufacturingOrderDTO dto)
    {
        var created = await _service.CreateOrder(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ManufacturingOrderDTO>> Update(int id, [FromBody] UpdateManufacturingOrderDTO dto)
    {
        var updated = await _service.UpdateOrder(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteOrder(id);
        return NoContent();
    }

    [HttpGet("{id}/graph")]
    public async Task<ActionResult<GraphDto>> GetGraph(int id)
    {
        var graph = await _service.BuildGraphAsync(id);
        if (graph == null || !graph.Nodes.Any())
            return NotFound();
        return Ok(graph);
    }
}