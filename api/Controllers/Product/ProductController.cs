using ApiTexPact.DTO;
using ApiTexPact.Services.Interface.Product;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _service;

    public ProductController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAll()
    {
        var products = await _service.GetAllProducts();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDTO>> GetById(int id)
    {
        var product = await _service.GetProductById(id);
        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDTO>> Create([FromBody] CreateProductDTO dto)
    {
        var created = await _service.CreateProduct(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDTO>> Update(int id, [FromBody] UpdateProductDTO dto)
    {
        var updated = await _service.UpdateProduct(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteProduct(id);
        return NoContent();
    }
}