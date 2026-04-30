using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("production_line")]
public class ProductionLineModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("location")]
    public string? Location { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("capacity")]
    public int? Capacity { get; set; }
}