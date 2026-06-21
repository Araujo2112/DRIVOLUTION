using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("config")]
public class ConfigModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("model_id")]
    [Required]
    public int ModelId { get; set; }

    [Column("item")]
    [Required]
    public string Item { get; set; } = string.Empty;
    
    [Column("allow_multiple")]
    public bool AllowMultiple { get; set; }

    // Navigation
    [ForeignKey("ModelId")]
    public CarModelModel CarModel { get; set; } = null!;

    public ICollection<ConfigOptionModel> ConfigOptions { get; set; } = new List<ConfigOptionModel>();
}
