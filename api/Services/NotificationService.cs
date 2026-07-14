using Drivolution.Data;
using Drivolution.DTO.Client;
using Drivolution.Models;
using Drivolution.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Services;

// Implementa o contrato definido em INotificationService
public class NotificationService : INotificationService
{
    // Contexto do Entity Framework usado para aceder à base de dados
    private readonly ApplicationDbContext _context;

    // Logger usado para registar erros e informação da aplicação
    private readonly ILogger<NotificationService> _logger;

    // O ASP.NET injeta automaticamente o contexto e o logger
    public NotificationService(
        ApplicationDbContext context,
        ILogger<NotificationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Cria uma nova notificação para um utilizador
    public async Task CreateAsync(
        int appUserId,
        string type,
        string message,
        int? clientOrderId = null,
        int? productId = null)
    {
        // Cria o objeto que será guardado na base de dados
        var notification = new NotificationModel
        {
            AppUserId = appUserId,
            Type = type,
            Message = message,

            // Estes dados são opcionais
            ClientOrderId = clientOrderId,
            ProductId = productId,

            // Todas as notificações começam como não lidas
            IsRead = false,

            // Guarda a data e hora de criação
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            // Adiciona a notificação ao contexto
            _context.Set<NotificationModel>().Add(notification);

            // Guarda a notificação na base de dados
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Regista o erro, mas não interrompe a operação principal
            _logger.LogError(
                ex,
                "Falha ao criar notificação: type={Type}, appUserId={AppUserId}. " +
                "A operação principal NÃO foi afetada por este erro.",
                type,
                appUserId
            );

            // Remove a entidade do controlo do Entity Framework
            // para evitar problemas em operações seguintes
            _context.Entry(notification).State = EntityState.Detached;
        }
    }

    // Verifica se já existe uma notificação de determinado tipo
    // para uma encomenda específica
    public async Task<bool> ExistsAsync(
        string type,
        int clientOrderId)
    {
        return await _context.Set<NotificationModel>()
            .AnyAsync(n =>
                n.Type == type &&
                n.ClientOrderId == clientOrderId
            );
    }

    // Devolve as notificações de um utilizador
    public async Task<IEnumerable<NotificationDTO>> GetForUserAsync(
        int appUserId,
        int limit = 20)
    {
        return await _context.Set<NotificationModel>()

            // Filtra apenas as notificações deste utilizador
            .Where(n => n.AppUserId == appUserId)

            // Ordena da mais recente para a mais antiga
            .OrderByDescending(n => n.CreatedAt)

            // Limita a quantidade de resultados
            .Take(limit)

            // Converte o Model para DTO
            .Select(n => new NotificationDTO
            {
                Id = n.Id,
                Type = n.Type,
                Message = n.Message,
                ClientOrderId = n.ClientOrderId,
                ProductId = n.ProductId,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
            })

            // Executa a consulta e transforma o resultado numa lista
            .ToListAsync();
    }

    // Conta quantas notificações ainda não foram lidas
    public async Task<int> CountUnreadAsync(int appUserId)
    {
        return await _context.Set<NotificationModel>()
            .CountAsync(n =>
                n.AppUserId == appUserId &&
                !n.IsRead
            );
    }

    // Marca uma notificação específica como lida
    public async Task<bool> MarkReadAsync(
        int id,
        int appUserId)
    {
        // Procura a notificação pelo ID e confirma
        // que pertence ao utilizador
        var notification =
            await _context.Set<NotificationModel>()
                .FirstOrDefaultAsync(n =>
                    n.Id == id &&
                    n.AppUserId == appUserId
                );

        // Se não existir, devolve false
        if (notification == null)
            return false;

        // Marca como lida
        notification.IsRead = true;

        // Guarda a alteração
        await _context.SaveChangesAsync();

        return true;
    }

    // Marca todas as notificações do utilizador como lidas
    public async Task MarkAllReadAsync(int appUserId)
    {
        await _context.Set<NotificationModel>()

            // Procura apenas as notificações não lidas deste utilizador
            .Where(n =>
                n.AppUserId == appUserId &&
                !n.IsRead
            )

            // Atualiza todas diretamente na base de dados
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    n => n.IsRead,
                    true
                )
            );
    }

    // Apaga todas as notificações de um utilizador
    public async Task DeleteAllAsync(int appUserId)
    {
        await _context.Set<NotificationModel>()

            // Seleciona todas as notificações do utilizador
            .Where(n => n.AppUserId == appUserId)

            // Apaga diretamente da base de dados
            .ExecuteDeleteAsync();
    }
}