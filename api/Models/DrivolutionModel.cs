using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("model")]
public class DrivolutionModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("version")]
    public string? Version { get; set; }

    [Column("type")]
    public string? Type { get; set; }
}