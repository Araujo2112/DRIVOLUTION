using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ManufacturingPhaseDTO
{
    [Required] public int Id { get; set; }
    [Required] public string PhaseInfo { get; set; }
    [Required] public int PhaseDuration { get; set; }
    [Required] public string ManufacturingPhaseId { get; set; }
    [Required] public int PlantFloorSectionId { get; set; }
    [Required] public string PlantFloorSectionName { get; set; }
}

public class CreateManufacturingPhaseDTO
{
    [Required] public string PhaseInfo { get; set; }
    [Required] public int PhaseDuration { get; set; }
    [Required] public int PlantFloorSectionId { get; set; }
}

public class UpdateManufacturingPhaseDTO
{
    [Required] public string PhaseInfo { get; set; }
    [Required] public int PhaseDuration { get; set; }
}

public class ManufacturingPhaseIdDTO
{
    [Required] public int Id { get; set; }
}

public class UpdateManufacturingPhaseWithIdDTO
{
    [Required] public int Id { get; set; }
    [Required] public string PhaseInfo { get; set; }
    [Required] public int PhaseDuration { get; set; }
}