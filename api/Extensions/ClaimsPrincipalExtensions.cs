using System.Security.Claims;

namespace Drivolution.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static (int Id, string Name) GetAuditUser(this ClaimsPrincipal user)
    {
        var idClaim   = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";
        var nameClaim = user.FindFirstValue("name") ?? "Desconhecido";
        int.TryParse(idClaim, out var id);
        return (id, nameClaim);
    }
}