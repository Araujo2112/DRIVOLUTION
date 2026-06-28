using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("audit_log")]
public class AuditLogModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    // Snapshot do nome — persiste mesmo que o utilizador seja apagado
    [Column("user_name")]
    [Required]
    public string UserName { get; set; } = string.Empty;

    // "created" | "updated" | "deleted"
    [Column("action")]
    [Required]
    public string Action { get; set; } = string.Empty;

    // "car_model" | "config" | "config_option" | "phase" | "phase_sequence"
    // "production_line" | "workstation" | "support" | "order" | "user"
    [Column("entity")]
    [Required]
    public string Entity { get; set; } = string.Empty;

    [Column("entity_id")]
    public int EntityId { get; set; }

    // Snapshot do nome/referência do objeto
    [Column("entity_label")]
    [Required]
    public string EntityLabel { get; set; } = string.Empty;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}