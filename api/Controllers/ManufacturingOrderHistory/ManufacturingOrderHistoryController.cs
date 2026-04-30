using ApiTexPact.DTO;
using ApiTexPact.Services;
using ApiTexPact.Services.Interface.ManufacturingOrderHistory;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers.ManufacturingOrderHistory;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingOrderHistoryController : ControllerBase
{
    private readonly IManufacturingOrderHistoryService _service;

    public ManufacturingOrderHistoryController(IManufacturingOrderHistoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ManufacturingOrderHistoryDTO>>> GetAll()
    {
        var histories = await _service.GetAllHistory();
        return Ok(histories);
    }

    [HttpGet("{plantFloorSectionId}/{manufacturingOrderId}")]
    public async Task<ActionResult<ManufacturingOrderHistoryDTO>> GetById(int plantFloorSectionId, int manufacturingOrderId)
    {
        var history = await _service.GetHistoryById(manufacturingOrderId, plantFloorSectionId);
        if (history == null)
            return NotFound();

        return Ok(history);
    }

    [HttpPost]
    public async Task<ActionResult<ManufacturingOrderHistoryDTO>> Create([FromBody] CreateManufacturingOrderHistoryDTO dto)
    {
        var created = await _service.CreateHistory(dto);
        return Ok(created);
    }

    [HttpPut("update/{plantFloorSectionId}/{manufacturingOrderId}")]
    public async Task<ActionResult<ManufacturingOrderHistoryDTO>> Update(int plantFloorSectionId, int manufacturingOrderId, [FromBody] UpdateManufacturingOrderHistoryDTO dto)
    {
        var updated = await _service.UpdateHistory(manufacturingOrderId, plantFloorSectionId, dto);
        return Ok(updated);
    }

    [HttpDelete("delete/{plantFloorSectionId}/{manufacturingOrderId}")]
    public async Task<IActionResult> Delete(int plantFloorSectionId, int manufacturingOrderId)
    {
        await _service.DeleteHistory(manufacturingOrderId, plantFloorSectionId);
        return NoContent();
    }
}
