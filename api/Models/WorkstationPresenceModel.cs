using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("workstation_presence")]
public class WorkstationPresenceModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("app_user_id")]
    [Required]
    public int AppUserId { get; set; }

    [Column("workstation_id")]
    [Required]
    public int WorkstationId { get; set; }

    [Column("checked_in_at")]
    [Required]
    public DateTime CheckedInAt { get; set; } = DateTime.UtcNow;

    [Column("checked_out_at")]
    public DateTime? CheckedOutAt { get; set; }

    // Navigation
    [ForeignKey("AppUserId")]
    public UserModel AppUser { get; set; } = null!;

    [ForeignKey("WorkstationId")]
    public WorkstationModel Workstation { get; set; } = null!;
}