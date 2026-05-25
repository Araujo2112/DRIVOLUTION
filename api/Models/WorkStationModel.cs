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
    [Required]
    public int ProductionLineId { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("manufacturing_phase_id")]
    public int? ManufacturingPhaseId { get; set; }

    // Navigation
    [ForeignKey("ProductionLineId")]
    public ProductionLineModel ProductionLine { get; set; } = null!;

    [ForeignKey("ManufacturingPhaseId")]
    public ManufacturingPhaseModel? ManufacturingPhase { get; set; }

    public ICollection<WorkstationStatusModel> WorkstationStatuses { get; set; } = new List<WorkstationStatusModel>();
    public ICollection<WorkstationAllocationModel> WorkstationAllocations { get; set; } = new List<WorkstationAllocationModel>();
    public ICollection<LocalizationHistoryModel> LocalizationHistories { get; set; } = new List<LocalizationHistoryModel>();
    public ICollection<ProductPhaseModel> ProductPhases { get; set; } = new List<ProductPhaseModel>();
}