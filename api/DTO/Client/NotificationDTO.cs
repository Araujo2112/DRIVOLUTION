namespace Drivolution.DTO.Client;

public class NotificationDTO
{
    public int Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public int? ClientOrderId { get; set; }

    public int? ProductId { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAt { get; set; }
}
