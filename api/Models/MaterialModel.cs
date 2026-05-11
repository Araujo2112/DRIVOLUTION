using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("material")]
public class MaterialModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("item")]
    [Required]
    public string Item { get; set; } = string.Empty;

    [Column("part_number")]
    public string? PartNumber { get; set; }

    // Navigation
    public ICollection<ModelMaterialModel> ModelMaterials { get; set; } = new List<ModelMaterialModel>();
}
