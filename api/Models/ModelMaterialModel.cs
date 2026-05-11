using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("model_material")]
public class ModelMaterialModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("model_id")]
    [Required]
    public int ModelId { get; set; }

    [Column("material_id")]
    [Required]
    public int MaterialId { get; set; }

    [Column("manufacturing_phase_id")]
    [Required]
    public int ManufacturingPhaseId { get; set; }

    [Column("quantity")]
    [Required]
    public decimal Quantity { get; set; }

    [Column("unit")]
    public string? Unit { get; set; }

    // Navigation
    [ForeignKey("ModelId")]
    public CarModelModel CarModel { get; set; } = null!;

    [ForeignKey("MaterialId")]
    public MaterialModel Material { get; set; } = null!;

    [ForeignKey("ManufacturingPhaseId")]
    public ManufacturingPhaseModel ManufacturingPhase { get; set; } = null!;
}
