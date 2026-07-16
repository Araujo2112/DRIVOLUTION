using Drivolution.DTO;
using Drivolution.Extensions;
using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/User
[Route("api/[controller]")]

// Apenas administradores podem gerir utilizadores
[Authorize(Roles = "admin")]
public class UserController : ControllerBase
{
    // Service responsável pela gestão dos utilizadores
    private readonly IUserService _userService;

    // O ASP.NET injeta automaticamente o service necessário
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    // GET /api/User
    // Devolve uma lista paginada de utilizadores, permitindo aplicar filtros
    [HttpGet]
    public async Task<IActionResult> GetPaged(
        // Número da página; por defeito começa na página 1
        [FromQuery] int page = 1,

        // Número de resultados apresentados por página
        [FromQuery] int pageSize = 25,

        // Pesquisa opcional por nome ou email
        [FromQuery] string? search = null,

        // Filtro opcional por role
        [FromQuery] string? role = null)
    {
        // Obtém os utilizadores de acordo com os filtros escolhidos
        var result = await _userService.GetTeamPagedAsync(page, pageSize, search, role);

        // Devolve HTTP 200 com os resultados
        return Ok(result);
    }

    // GET /api/User/clients — lista de contas "client" ativas, para dropdowns
    [HttpGet("clients")]
    // Administradores e gestores podem consultar os clientes ativos
    [Authorize(Roles = "admin,manager")]
    public async Task<IActionResult> GetClients()
    {
        // Obtém todos os clientes ativos
        var clients = await _userService.GetActiveClientsAsync();

        // Devolve a lista de clientes
        return Ok(clients);
    }

    // PUT /api/User/{id}
    // Atualiza um utilizador existente
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserRequestDTO dto)
    {
        // Obtém o utilizador autenticado para efeitos de auditoria
        var (auditUserId, auditUserName) = User.GetAuditUser();

        try
        {
            // Atualiza o utilizador
            var result = await _userService.UpdateAsync(id, dto, auditUserId, auditUserName);

            // Se o utilizador não existir ou não puder ser editado devolve 404
            if (result is null)
                return NotFound("Utilizador não encontrado ou não editável.");

            // Devolve o utilizador atualizado
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            // Devolve erro caso os dados enviados sejam inválidos
            return BadRequest(ex.Message);
        }
    }

    // POST /api/User/{id}/reset-password
    // Gera uma nova password temporária para um utilizador
    [HttpPost("{id:int}/reset-password")]
    public async Task<IActionResult> ResetPassword(int id)
    {
        // Obtém o utilizador autenticado para efeitos de auditoria
        var (auditUserId, auditUserName) = User.GetAuditUser();

        // Gera uma nova password temporária
        var temporaryPassword = await _userService.ResetPasswordAsync(
            id,
            auditUserId,
            auditUserName
        );

        // Se o utilizador não existir ou não puder ser editado devolve 404
        if (temporaryPassword is null)
            return NotFound("Utilizador não encontrado ou não editável.");

        // Devolve a nova password temporária
        return Ok(new ResetPasswordResponseDTO
        {
            TemporaryPassword = temporaryPassword
        });
    }
}