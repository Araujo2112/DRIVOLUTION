using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ManufacturingProcessPhaseDTO
{
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }
    [Required] public int NumberStepOrder { get; set; }
    [Required] public string ManufacturingProcessName { get; set; }
    [Required] public string ManufacturingPhaseName { get; set; }
}

public class CreateManufacturingProcessPhaseDTO
{
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }
    [Required] public int NumberStepOrder { get; set; }
}

public class UpdateManufacturingProcessPhaseDTO
{
    [Required] public int NumberStepOrder { get; set; }
}

public class ManufacturingProcessPhaseCompositeIdsDTO
{
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }
}

public class UpdateManufacturingProcessPhaseWithIdsDTO
{
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }
    [Required] public int NumberStepOrder { get; set; }
}