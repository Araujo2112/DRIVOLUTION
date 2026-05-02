using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("phase_workstation")]
public class PhaseWorkstationModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("manufacturing_phase_id")]
    public int ManufacturingPhaseId { get; set; }

    [Column("workstation_id")]
    public int WorkstationId { get; set; }

    public DrivolutionManufacturingPhaseModel? ManufacturingPhase { get; set; }

    public WorkstationModel? Workstation { get; set; }
}