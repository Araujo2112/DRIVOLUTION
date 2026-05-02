using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("manufacturing_phase")]
public class DrivolutionManufacturingPhaseModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("estimated_duration")]
    public int? EstimatedDuration { get; set; }
}