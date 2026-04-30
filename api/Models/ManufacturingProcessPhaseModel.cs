using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Models;

[PrimaryKey(nameof(ManufacturingPhaseId), nameof(ManufacturingProcessId))]
public class ManufacturingProcessPhaseModel
{
    [ForeignKey("ManufacturingPhase")]
    public int ManufacturingPhaseId { get; set; }

    [ForeignKey("ManufacturingProcess")]
    public int ManufacturingProcessId { get; set; }

    public int NumberStepOrder { get; set; }
    
    public ManufacturingProcessModel ManufacturingProcess { get; set; }

    public ManufacturingPhaseModel ManufacturingPhase { get; set; }
}