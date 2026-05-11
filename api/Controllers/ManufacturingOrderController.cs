using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingOrder;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ManufacturingOrderController : ControllerBase
{
    private readonly IManufacturingOrderRepository _repo;
    public ManufacturingOrderController(IManufacturingOrderRepository repo) => _repo = repo;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(mo => new ManufacturingOrderDTO(mo.Id, mo.ClientOrderId, mo.ClientOrder?.CustomerName ?? "", mo.ManufacturingOrderNumber, mo.StartDate, mo.EndDate, mo.Status)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ManufacturingOrderDTO(item.Id, item.ClientOrderId, item.ClientOrder?.CustomerName ?? "", item.ManufacturingOrderNumber, item.StartDate, item.EndDate, item.Status));
    }

    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetWithDetails(int id)
    {
        var item = await _repo.GetByIdWithDetails(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(string status)
    {
        var items = await _repo.GetByStatus(status);
        return Ok(items.Select(mo => new ManufacturingOrderDTO(mo.Id, mo.ClientOrderId, mo.ClientOrder?.CustomerName ?? "", mo.ManufacturingOrderNumber, mo.StartDate, mo.EndDate, mo.Status)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManufacturingOrderDTO dto)
    {
        var entity = new ManufacturingOrderModel
        {
            ClientOrderId = dto.ClientOrderId,
            ManufacturingOrderNumber = dto.ManufacturingOrderNumber,
            StartDate = dto.StartDate,
            Status = "pending"
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ManufacturingOrderDTO(created.Id, created.ClientOrderId, "", created.ManufacturingOrderNumber, created.StartDate, created.EndDate, created.Status));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateManufacturingOrderDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.EndDate != null) entity.EndDate = dto.EndDate;
        await _repo.Update(entity);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!await _repo.Exists(id)) return NotFound();
        await _repo.Delete(id);
        return NoContent();
    }
}