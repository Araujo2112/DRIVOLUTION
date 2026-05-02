using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("workstation")]
public class WorkstationModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("production_line_id")]
    public int ProductionLineId { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    public ProductionLineModel? ProductionLine { get; set; }
}