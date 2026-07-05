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
        private readonly INotificationService _notificationService;

        public ClientPortalController(IClientPortalService service, INotificationService notificationService)
        {
            _service = service;
            _notificationService = notificationService;
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

        // ─── Fluxo "Nova Encomenda" (seleção de modelo + configurador) ───
        // Só leitura: CarModel/Config/ConfigOption continuam geridos em
        // exclusivo pelo admin nos seus próprios controllers.

        // GET /api/client/models
        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            return Ok(await _service.GetModelsAsync());
        }

        // GET /api/client/models/{id}
        [HttpGet("models/{id}")]
        public async Task<IActionResult> GetModel(int id)
        {
            var model = await _service.GetModelAsync(id);
            if (model == null) return NotFound();
            return Ok(model);
        }

        // GET /api/client/models/{id}/configs
        [HttpGet("models/{id}/configs")]
        public async Task<IActionResult> GetModelConfigs(int id)
        {
            var configs = await _service.GetModelConfigsAsync(id);
            if (configs == null) return NotFound();
            return Ok(configs);
        }

        // ─── Notificações (sino no cabeçalho — Card N) ───

        // GET /api/client/notifications
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var (userId, _) = User.GetAuditUser();
            if (userId == 0) return Unauthorized();

            var notifications = await _notificationService.GetForUserAsync(userId);
            var unreadCount = await _notificationService.CountUnreadAsync(userId);
            return Ok(new { items = notifications, unreadCount });
        }

        // POST /api/client/notifications/{id}/read
        [HttpPost("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var (userId, _) = User.GetAuditUser();
            if (userId == 0) return Unauthorized();

            var ok = await _notificationService.MarkReadAsync(id, userId);
            if (!ok) return NotFound();
            return NoContent();
        }

        // POST /api/client/notifications/read-all
        [HttpPost("notifications/read-all")]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var (userId, _) = User.GetAuditUser();
            if (userId == 0) return Unauthorized();

            await _notificationService.MarkAllReadAsync(userId);
            return NoContent();
        }

        // DELETE /api/client/notifications
        [HttpDelete("notifications")]
        public async Task<IActionResult> ClearAllNotifications()
        {
            var (userId, _) = User.GetAuditUser();
            if (userId == 0) return Unauthorized();

            await _notificationService.DeleteAllAsync(userId);
            return NoContent();
        }
    }
}