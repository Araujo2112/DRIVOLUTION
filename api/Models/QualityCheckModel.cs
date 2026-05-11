using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("quality_check")]
public class QualityCheckModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("product_id")]
    [Required]
    public int ProductId { get; set; }

    [Column("manufacturing_phase_id")]
    [Required]
    public int ManufacturingPhaseId { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    // Navigation
    [ForeignKey("ProductId")]
    public ProductModel Product { get; set; } = null!;

    [ForeignKey("ManufacturingPhaseId")]
    public ManufacturingPhaseModel ManufacturingPhase { get; set; } = null!;

    public ICollection<ProductPhaseModel> ProductPhases { get; set; } = new List<ProductPhaseModel>();
}
