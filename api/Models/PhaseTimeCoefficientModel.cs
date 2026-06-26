using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("phase_time_coefficient")]
public class PhaseTimeCoefficientModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("manufacturing_phase_id")]
    [Required]
    public int ManufacturingPhaseId { get; set; }

    [Column("config_option_id")]
    public int? ConfigOptionId { get; set; }

    [Column("production_line_id")]
    public int? ProductionLineId { get; set; }

    [Column("model_id")]
    public int? ModelId { get; set; }

    [Column("weight_seconds")]
    [Required]
    public decimal WeightSeconds { get; set; }

    [Column("trained_at")]
    [Required]
    public DateTime TrainedAt { get; set; }

    // Navigation
    [ForeignKey("ManufacturingPhaseId")]
    public ManufacturingPhaseModel ManufacturingPhase { get; set; } = null!;

    [ForeignKey("ConfigOptionId")]
    public ConfigOptionModel? ConfigOption { get; set; }

    [ForeignKey("ProductionLineId")]
    public ProductionLineModel? ProductionLine { get; set; }

    [ForeignKey("ModelId")]
    public CarModelModel? CarModel { get; set; }
}