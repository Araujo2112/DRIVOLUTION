using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("support")]
public class SupportModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("production_line_id")]
    [Required]
    public int ProductionLineId { get; set; }

    [Column("rfid_tag")]
    public string? RfidTag { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    // Navigation
    [ForeignKey("ProductionLineId")]
    public ProductionLineModel ProductionLine { get; set; } = null!;

    public ICollection<LocalizationHistoryModel> LocalizationHistories { get; set; } = new List<LocalizationHistoryModel>();
    public ICollection<SupportedProductModel> SupportedProducts { get; set; } = new List<SupportedProductModel>();
}
