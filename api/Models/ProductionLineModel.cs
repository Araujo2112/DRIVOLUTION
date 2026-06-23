using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("production_line")]
public class ProductionLineModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    public string Name { get; set; } = string.Empty;

    [Column("location")]
    public string? Location { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("capacity")]
    public int? Capacity { get; set; }

    // Navigation
    public ICollection<WorkstationModel> Workstations { get; set; } = new List<WorkstationModel>();
    public ICollection<SupportModel> Supports { get; set; } = new List<SupportModel>();
}
