using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("phase_sequence")]
public class PhaseSequenceModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order")]
    [Required]
    public int Order { get; set; }

    [Column("manufacturing_phase_id")]
    [Required]
    public int ManufacturingPhaseId { get; set; }

    [Column("model_id")]
    [Required]
    public int ModelId { get; set; }

    // Navigation
    [ForeignKey("ManufacturingPhaseId")]
    public ManufacturingPhaseModel ManufacturingPhase { get; set; } = null!;

    [ForeignKey("ModelId")]
    public CarModelModel CarModel { get; set; } = null!;
}
