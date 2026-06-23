using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("alert")]
public class AlertModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("type")]
    [Required]
    public string Type { get; set; } = null!;  // "time_exceeded" / "wrong_sequence"

    [Column("status")]
    [Required]
    public string Status { get; set; } = "open";  // "open" / "acknowledged" / "resolved"

    [Column("product_id")]
    [Required]
    public int ProductId { get; set; }

    [Column("product_phase_id")]
    [Required]
    public int ProductPhaseId { get; set; }

    [Column("triggered_at")]
    [Required]
    public DateTime TriggeredAt { get; set; }

    [Column("acknowledged_at")]
    public DateTime? AcknowledgedAt { get; set; }

    [Column("resolved_at")]
    public DateTime? ResolvedAt { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("product_serial")]
    [Required]
    public string ProductSerial { get; set; } = null!;

    [Column("phase_name")]
    [Required]
    public string PhaseName { get; set; } = null!;

    [Column("threshold_pct")]
    public int? ThresholdPct { get; set; }

    [Column("estimated_duration")]
    public int? EstimatedDuration { get; set; }

    [Column("order_from")]
    public int? OrderFrom { get; set; }

    [Column("order_to")]
    public int? OrderTo { get; set; }

    // Navigation
    [ForeignKey("ProductId")]
    public ProductModel Product { get; set; } = null!;

    [ForeignKey("ProductPhaseId")]
    public ProductPhaseModel ProductPhase { get; set; } = null!;
}