using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("workstation_allocation")]
public class WorkstationAllocationModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("resource_id")]
    [Required]
    public int ResourceId { get; set; }

    [Column("workstation_id")]
    [Required]
    public int WorkstationId { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    // Navigation
    [ForeignKey("ResourceId")]
    public ResourceModel Resource { get; set; } = null!;

    [ForeignKey("WorkstationId")]
    public WorkstationModel Workstation { get; set; } = null!;
}
