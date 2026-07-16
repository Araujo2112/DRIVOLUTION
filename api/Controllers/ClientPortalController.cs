using Drivolution.Extensions;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers
{
    // Indica que esta classe é um controller da API
    [ApiController]

    // Define a rota base: /api/client
    [Route("api/client")]

    // Apenas utilizadores com o papel de cliente podem aceder
    [Authorize(Roles = "client")]
    public class ClientPortalController : ControllerBase
    {
        // Service responsável pela lógica do Portal do Cliente
        private readonly IClientPortalService _service;

        // Service responsável pela gestão das notificações
        private readonly INotificationService _notificationService;

        // O ASP.NET injeta automaticamente os services necessários
        public ClientPortalController(IClientPortalService service, INotificationService notificationService)
        {
            _service = service;
            _notificationService = notificationService;
        }

        // GET /api/client/orders
        // Devolve todas as encomendas do cliente autenticado
        [HttpGet("orders")]
        public async Task<IActionResult> GetOrders()
        {
            // Obtém o ID do utilizador autenticado
            var (userId, _) = User.GetAuditUser();

            if (userId == 0)
                return Unauthorized();

            // Obtém apenas as encomendas pertencentes a este cliente
            var orders = await _service.GetOrdersAsync(userId);

            return Ok(orders);
        }

        // GET /api/client/orders/{id}/products
        // Devolve os detalhes de uma encomenda específica do cliente
        [HttpGet("orders/{id}/products")]
        public async Task<IActionResult> GetOrderDetail(int id)
        {
            var (userId, _) = User.GetAuditUser();

            if (userId == 0)
                return Unauthorized();

            // Procura os detalhes da encomenda, verificando que pertence ao cliente
            var detail = await _service.GetOrderDetailAsync(id, userId);

            if (detail == null)
                return NotFound();

            return Ok(detail);
        }

        // ─── Fluxo "Nova Encomenda" (seleção de modelo + configurador) ───
        // Só leitura: CarModel/Config/ConfigOption continuam geridos em
        // exclusivo pelo admin nos seus próprios controllers.

        // GET /api/client/models
        // Devolve todos os modelos de veículos disponíveis
        [HttpGet("models")]
        public async Task<IActionResult> GetModels()
        {
            return Ok(await _service.GetModelsAsync());
        }

        // GET /api/client/models/{id}
        // Devolve um modelo específico
        [HttpGet("models/{id}")]
        public async Task<IActionResult> GetModel(int id)
        {
            var model = await _service.GetModelAsync(id);

            if (model == null)
                return NotFound();

            return Ok(model);
        }

        // GET /api/client/models/{id}/configs
        // Devolve as configurações disponíveis para um modelo
        [HttpGet("models/{id}/configs")]
        public async Task<IActionResult> GetModelConfigs(int id)
        {
            var configs = await _service.GetModelConfigsAsync(id);

            if (configs == null)
                return NotFound();

            return Ok(configs);
        }

        // ─── Notificações (sino no cabeçalho — Card N) ───

        // GET /api/client/notifications
        // Devolve todas as notificações do cliente e o número de notificações por ler
        [HttpGet("notifications")]
        public async Task<IActionResult> GetNotifications()
        {
            var (userId, _) = User.GetAuditUser();

            if (userId == 0)
                return Unauthorized();

            // Obtém as notificações do utilizador
            var notifications = await _notificationService.GetForUserAsync(userId);

            // Conta quantas ainda não foram lidas
            var unreadCount = await _notificationService.CountUnreadAsync(userId);

            return Ok(new
            {
                items = notifications,
                unreadCount
            });
        }

        // POST /api/client/notifications/{id}/read
        // Marca uma notificação como lida
        [HttpPost("notifications/{id}/read")]
        public async Task<IActionResult> MarkNotificationRead(int id)
        {
            var (userId, _) = User.GetAuditUser();

            if (userId == 0)
                return Unauthorized();

            var ok = await _notificationService.MarkReadAsync(id, userId);

            if (!ok)
                return NotFound();

            return NoContent();
        }

        // POST /api/client/notifications/read-all
        // Marca todas as notificações do cliente como lidas
        [HttpPost("notifications/read-all")]
        public async Task<IActionResult> MarkAllNotificationsRead()
        {
            var (userId, _) = User.GetAuditUser();

            if (userId == 0)
                return Unauthorized();

            await _notificationService.MarkAllReadAsync(userId);

            return NoContent();
        }

        // DELETE /api/client/notifications
        // Remove todas as notificações do cliente
        [HttpDelete("notifications")]
        public async Task<IActionResult> ClearAllNotifications()
        {
            var (userId, _) = User.GetAuditUser();

            if (userId == 0)
                return Unauthorized();

            await _notificationService.DeleteAllAsync(userId);

            return NoContent();
        }
    }
}