using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin,manager,operator")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly IEtaPredictionService _etaService;
    public ProductController(IProductRepository repo, IEtaPredictionService etaService)
    {
        _repo = repo;
        _etaService = etaService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAll();
        return Ok(items.Select(p => new ProductDTO(p.Id, p.ManufacturingOrderId, p.ModelId, p.CarModel?.Name, p.SerialNumber, p.LotNumber, p.ProductionDate)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _repo.GetById(id);
        if (item == null) return NotFound();
        return Ok(new ProductDTO(item.Id, item.ManufacturingOrderId, item.ModelId, item.CarModel?.Name, item.SerialNumber, item.LotNumber, item.ProductionDate));
    }

    [HttpGet("order/{orderId}")]
    public async Task<IActionResult> GetByOrder(int orderId)
    {
        var items = await _repo.GetByManufacturingOrder(orderId);
        return Ok(items.Select(p => new ProductDTO(p.Id, p.ManufacturingOrderId, p.ModelId, p.CarModel?.Name, p.SerialNumber, p.LotNumber, p.ProductionDate)));
    }

    [HttpGet("{id}/eta")]
    public async Task<IActionResult> GetEta(int id)
    {
        var result = await _etaService.PredictForProduct(id);
        if (result == null) return NotFound();
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDTO dto)
    {
        var entity = new ProductModel
        {
            ManufacturingOrderId = dto.ManufacturingOrderId,
            ModelId = dto.ModelId,
            SerialNumber = dto.SerialNumber,
            LotNumber = dto.LotNumber,
        };
        var created = await _repo.Create(entity);
        return CreatedAtAction(nameof(GetById), new { id = created.Id },
            new ProductDTO(created.Id, created.ManufacturingOrderId, created.ModelId, null, created.SerialNumber, created.LotNumber, created.ProductionDate));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateProductDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return NotFound();
        if (dto.ProductionDate != null) entity.ProductionDate = dto.ProductionDate;
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