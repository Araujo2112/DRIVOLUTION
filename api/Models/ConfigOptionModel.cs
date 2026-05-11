using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("config_option")]
public class ConfigOptionModel {
    [Key, Column("id")] 
    public int Id { get; set; }
    [Column("config_id")] 
    public int ConfigId { get; set; }
    [Column("value")] 
    public string Value { get; set; } = string.Empty;
    [Column("is_default")] 
    public bool IsDefault { get; set; }
    [ForeignKey("ConfigId")] 
    public ConfigModel Config { get; set; } = null!;
}