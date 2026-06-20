using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("product_phase")]
public class ProductPhaseModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("result")]
    public string? Result { get; set; }

    [Column("datetime_ini")]
    [Required]
    public DateTime DatetimeIni { get; set; }

    [Column("datetime_end")]
    public DateTime? DatetimeEnd { get; set; }

    [Column("manufacturing_phase_id")]
    [Required]
    public int ManufacturingPhaseId { get; set; }

    [Column("product_id")]
    [Required]
    public int ProductId { get; set; }

    [Column("workstation_id")]
    [Required]
    public int WorkstationId { get; set; }

    [Column("quality_id")]
    public int? QualityId { get; set; }

    // Navigation
    [ForeignKey("ManufacturingPhaseId")]
    public ManufacturingPhaseModel ManufacturingPhase { get; set; } = null!;

    [ForeignKey("ProductId")]
    public ProductModel Product { get; set; } = null!;

    [ForeignKey("WorkstationId")]
    public WorkstationModel Workstation { get; set; } = null!;

    [ForeignKey("QualityId")]
    public QualityCheckModel? QualityCheck { get; set; }
    public ICollection<AlertModel> Alerts { get; set; } = new List<AlertModel>();
}
