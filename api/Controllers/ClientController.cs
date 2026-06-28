using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/client")]
[Authorize(Roles = "client")]
public class ClientController : ControllerBase
{
    private readonly IClientPortalService _service;

    public ClientController(IClientPortalService service)
    {
        _service = service;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetOrders()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _service.GetOrders(userId);

        return Ok(result);
    }

    [HttpGet("orders/{orderId}/products")]
    public async Task<IActionResult> GetProducts(int orderId)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        var result = await _service.GetProducts(userId, orderId);

        return Ok(result);
    }
}