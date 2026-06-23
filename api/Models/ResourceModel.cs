using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("resource")]
public class ResourceModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("is_human")]
    [Required]
    public bool IsHuman { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    // Navigation
    public ICollection<WorkstationAllocationModel> WorkstationAllocations { get; set; } = new List<WorkstationAllocationModel>();
}
