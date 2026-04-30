using ApiTexPact.DTO;
using ApiTexPact.Services.Interface.ProductLot;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers.ProductLot
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductLotController : ControllerBase
    {
        private readonly IProductLotService _service;

        public ProductLotController(IProductLotService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductLotDTO>>> GetAll()
        {
            var productLots = await _service.GetAllProductLots();
            return Ok(productLots);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductLotDTO>> GetById(int id)
        {
            var productLot = await _service.GetProductLotById(id);
            if (productLot == null)
                return NotFound();

            return Ok(productLot);
        }

        [HttpPost]
        public async Task<ActionResult<ProductLotDTO>> Create([FromBody] CreateProductLotDTO dto)
        {
            var created = await _service.CreateProductLot(dto);
            return Ok(created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ProductLotDTO>> Update(int id, [FromBody] UpdateProductLotWithIdDTO dto)
        {
            var updated = await _service.UpdateProductLot(id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteProductLot(id);
            return NoContent();
        }
    }
}