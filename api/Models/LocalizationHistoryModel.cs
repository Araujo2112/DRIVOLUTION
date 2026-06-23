using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("localization_history")]
public class LocalizationHistoryModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("support_id")]
    [Required]
    public int SupportId { get; set; }

    [Column("workstation_id")]
    [Required]
    public int WorkstationId { get; set; }

    [Column("datetime_ini")]
    [Required]
    public DateTime DatetimeIni { get; set; }

    [Column("datetime_end")]
    public DateTime? DatetimeEnd { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    // Navigation
    [ForeignKey("SupportId")]
    public SupportModel Support { get; set; } = null!;

    [ForeignKey("WorkstationId")]
    public WorkstationModel Workstation { get; set; } = null!;
}
