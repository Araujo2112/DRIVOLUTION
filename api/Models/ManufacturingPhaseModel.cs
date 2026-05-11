using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("manufacturing_phase")]
public class ManufacturingPhaseModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("estimated_duration")]
    public int? EstimatedDuration { get; set; }

    // Navigation
    public ICollection<PhaseSequenceModel> PhaseSequences { get; set; } = new List<PhaseSequenceModel>();
    public ICollection<ProductPhaseModel> ProductPhases { get; set; } = new List<ProductPhaseModel>();
    public ICollection<QualityCheckModel> QualityChecks { get; set; } = new List<QualityCheckModel>();
    public ICollection<ModelMaterialModel> ModelMaterials { get; set; } = new List<ModelMaterialModel>();
}
