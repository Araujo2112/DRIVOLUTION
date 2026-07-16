/*
define um método de extensão para obter rapidamente o ID e o nome do utilizador autenticado a partir do token JWT, 
simplificando operações de auditoria e registo de ações na aplicação.
*/
using System.Security.Claims;

namespace Drivolution.Extensions;

// Classe estática que adiciona métodos extra à classe ClaimsPrincipal
public static class ClaimsPrincipalExtensions
{
    // Método de extensão que obtém o ID e o nome do utilizador autenticado
    public static (int Id, string Name) GetAuditUser(this ClaimsPrincipal user)
    {
        // Procura o claim que contém o ID do utilizador.
        // Se não existir, assume "0".
        var idClaim = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0";

        // Procura o claim personalizado que contém o nome.
        // Se não existir, devolve "Desconhecido".
        var nameClaim = user.FindFirstValue("name") ?? "Desconhecido";

        // Converte o ID recebido (string) para inteiro.
        // Se a conversão falhar, o valor fica a 0.
        int.TryParse(idClaim, out var id);

        // Devolve o ID e o nome sob a forma de uma tupla
        return (id, nameClaim);
    }
}