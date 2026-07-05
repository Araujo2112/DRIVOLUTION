using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("notification")]
public class NotificationModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("app_user_id")]
    [Required]
    public int AppUserId { get; set; }

    // "order_started" | "order_completed" | "car_completed"
    [Column("type")]
    [Required]
    public string Type { get; set; } = string.Empty;

    [Column("message")]
    [Required]
    public string Message { get; set; } = string.Empty;

    [Column("client_order_id")]
    public int? ClientOrderId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("is_read")]
    public bool IsRead { get; set; } = false;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
