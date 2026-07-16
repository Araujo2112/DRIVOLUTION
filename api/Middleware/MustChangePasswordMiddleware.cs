using System.Security.Claims;
using System.Text.Json;
using Drivolution.Repository.Interface;

namespace Drivolution.Middleware;

/// <summary>
/// Bloqueia o uso da API a qualquer utilizador autenticado com MustChangePassword = true,
/// exceto nos endpoints necessários para trocar a password e para saber quem está autenticado.
///
/// Verifica o estado diretamente na base de dados (e não uma claim gravada no JWT),
/// porque o token emitido no login não é renovado depois de trocar a password — se a claim
/// fosse a fonte de verdade, o utilizador continuaria bloqueado até o token expirar (8h).
/// </summary>
public class MustChangePasswordMiddleware
{
    // Lista de endpoints que continuam acessíveis mesmo quando
    // o utilizador ainda tem de alterar a password.
    private static readonly string[] AllowedPaths =
    {
        "/api/auth/login",
        "/api/auth/change-password",
        "/api/auth/me",
    };

    // Guarda uma referência para o próximo middleware
    // da sequência de processamento do pedido.
    private readonly RequestDelegate _next;

    // O ASP.NET injeta automaticamente o próximo middleware
    public MustChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Método executado automaticamente em cada pedido HTTP
    public async Task InvokeAsync(
        HttpContext context,
        IUserRepository userRepository)
    {
        // Só faz esta verificação se o utilizador estiver autenticado
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Obtém o caminho do pedido atual.
            // É convertido para minúsculas para facilitar a comparação.
            var path =
                context.Request.Path.Value?.ToLowerInvariant()
                ?? string.Empty;

            // Se o endpoint atual não estiver na lista de exceções,
            // verifica se o utilizador tem de alterar a password.
            if (!AllowedPaths.Contains(path))
            {
                // Obtém do token JWT o claim que contém o ID do utilizador
                var userIdClaim =
                    context.User
                        .FindFirst(ClaimTypes.NameIdentifier)?
                        .Value;

                // Confirma que o claim existe e que pode ser convertido para inteiro
                if (
                    userIdClaim is not null &&
                    int.TryParse(userIdClaim, out var userId)
                )
                {
                    // Procura o utilizador diretamente na base de dados
                    var user =
                        await userRepository.GetByIdAsync(userId);

                    // Se o utilizador existir e ainda tiver
                    // uma password temporária, bloqueia o pedido.
                    if (
                        user is not null &&
                        user.MustChangePassword
                    )
                    {
                        // Devolve HTTP 403 Forbidden
                        context.Response.StatusCode =
                            StatusCodes.Status403Forbidden;

                        // Indica que a resposta está em JSON
                        context.Response.ContentType =
                            "application/json";

                        // Cria a resposta que será enviada ao frontend
                        var payload =
                            JsonSerializer.Serialize(new
                            {
                                code =
                                    "PASSWORD_CHANGE_REQUIRED",

                                message =
                                    "Tens de trocar a password temporária antes de continuares.",
                            });

                        // Escreve o JSON na resposta HTTP
                        await context.Response.WriteAsync(payload);

                        // Interrompe o processamento.
                        // O controller e os middlewares seguintes não são executados.
                        return;
                    }
                }
            }
        }

        // Se o pedido não foi bloqueado,
        // passa-o para o próximo middleware
        await _next(context);
    }
}