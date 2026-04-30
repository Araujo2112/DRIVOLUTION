using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ManufacturingOrderHistoryDTO
{
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int PlantFloorSectionId { get; set; }
    [Required] public DateTime DateTime { get; set; }
    [Required] public string StatusName { get; set; }
}

public class CreateManufacturingOrderHistoryDTO
{
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int PlantFloorSectionId { get; set; }
    [Required] public DateTime DateTime { get; set; }
    [Required] public string StatusName { get; set; }
}

public class UpdateManufacturingOrderHistoryDTO
{
    [Required] public DateTime DateTime { get; set; }
    [Required] public string StatusName { get; set; }
}

// For Gets and any endpoint that uses the id's by body:

public class ManufacturingOrderHistoryKeyDTO
{
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int PlantFloorSectionId { get; set; }
}

public class UpdateManufacturingOrderHistoryWithKeyDTO
{
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int PlantFloorSectionId { get; set; }
    [Required] public DateTime DateTime { get; set; }
    [Required] public string StatusName { get; set; }
}