using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("workstation_status")]
public class WorkstationStatusModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("workstation_id")]
    [Required]
    public int WorkstationId { get; set; }

    [Column("status")]
    [Required]
    public string Status { get; set; } = string.Empty;

    [Column("timestamp")]
    [Required]
    public DateTime Timestamp { get; set; }

    // Navigation
    [ForeignKey("WorkstationId")]
    public WorkstationModel Workstation { get; set; } = null!;
}
