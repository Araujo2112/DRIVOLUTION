using ApiTexPact.DTO;
using ApiTexPact.Services.Interface.Client;
using Microsoft.AspNetCore.Mvc;

namespace ApiTexPact.Controllers.Client;

[ApiController]
[Route("api/[controller]")]
public class ClientController : ControllerBase
{
    private readonly IClientService _service;

    public ClientController(IClientService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDTO>>> GetAll()
    {
        var clients = await _service.GetAllClients();
        return Ok(clients);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDTO>> GetById(int id)
    {
        var client = await _service.GetClientById(id);
        if (client == null)
            return NotFound();

        return Ok(client);
    }

    [HttpPost]
    public async Task<ActionResult<ClientDTO>> Create([FromBody] CreateClientDTO dto)
    {
        var created = await _service.CreateClient(dto);
        return Ok(created);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDTO>> Update(int id, [FromBody] UpdateClientDTO dto)
    {
        var updated = await _service.UpdateClient(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteClient(id);
        return NoContent();
    }
}