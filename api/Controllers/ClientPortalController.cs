using Drivolution.Extensions;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers
{
    [ApiController]
    [Route("api/client")]
    [Authorize(Roles = "client")]
    public class ClientPortalController : ControllerBase
    {
        private readonly IClientPortalService _service;

        public ClientPortalController(IClientPortalService service)
        {
            _service = service;
        }

        // GET /api/client/orders
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            var (userId, _) = User.GetAuditUser();
            if (userId == 0) return Unauthorized();

            var orders = await _service.GetOrdersAsync(userId);
            return Ok(orders);
        }

        // GET /api/client/orders/{id}/products
        [HttpGet("orders/{id}/products")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var (userId, _) = User.GetAuditUser();
            if (userId == 0) return Unauthorized();

            var detail = await _service.GetOrderDetailAsync(id, userId);
            if (detail == null) return NotFound();

            return Ok(detail);
        }
    }
}