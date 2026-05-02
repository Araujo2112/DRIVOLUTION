using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("phase_sequence")]
public class PhaseSequenceModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order")]
    public int Order { get; set; }

    [Column("manufacturing_phase_id")]
    public int ManufacturingPhaseId { get; set; }

    [Column("model_id")]
    public int ModelId { get; set; }

    public DrivolutionManufacturingPhaseModel? ManufacturingPhase { get; set; }

    public DrivolutionModel? Model { get; set; }
}