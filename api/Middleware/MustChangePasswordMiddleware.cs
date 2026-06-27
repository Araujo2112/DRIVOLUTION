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
    private static readonly string[] AllowedPaths =
    {
        "/api/auth/login",
        "/api/auth/change-password",
        "/api/auth/me",
    };

    private readonly RequestDelegate _next;

    public MustChangePasswordMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

            if (!AllowedPaths.Contains(path))
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim is not null && int.TryParse(userIdClaim, out var userId))
                {
                    var user = await userRepository.GetByIdAsync(userId);

                    if (user is not null && user.MustChangePassword)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";

                        var payload = JsonSerializer.Serialize(new
                        {
                            code = "PASSWORD_CHANGE_REQUIRED",
                            message = "Tens de trocar a password temporária antes de continuares.",
                        });

                        await context.Response.WriteAsync(payload);
                        return;
                    }
                }
            }
        }

        await _next(context);
    }
}