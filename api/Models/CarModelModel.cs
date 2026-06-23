using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("model")]
public class CarModelModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("version")]
    public string? Version { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    // Navigation
    public ICollection<PhaseSequenceModel> PhaseSequences { get; set; } = new List<PhaseSequenceModel>();
    public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
    public ICollection<ModelMaterialModel> ModelMaterials { get; set; } = new List<ModelMaterialModel>();
    public ICollection<ConfigModel> Configs { get; set; } = new List<ConfigModel>();
}
