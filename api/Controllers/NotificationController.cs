using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using ApiTexPact.Services;
using ApiTexPact.Services.Interface;

namespace ApiTexPact.Controllers
{
    [ApiController]
    [Route("api/notification")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveNotification([FromBody] JsonElement notification)
        {
            try
            {
                if (notification.ValueKind == JsonValueKind.Undefined || notification.ValueKind == JsonValueKind.Null)
                {
                    Console.WriteLine("The received notification is empty or undefined.");
                    return BadRequest("Invalid notification or empty body.");
                }

                var success = await _notificationService.ProcessNotification(notification);
                if (success)
                {
                    return Ok("Notification processed successfully.");
                }
                else
                {
                    return BadRequest("Failed to process the notification.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error processing the notification: " + ex.Message);
                return StatusCode(500, "Internal server error.");
            }
        }
    }
}