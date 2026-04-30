using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

public class ManufacturingOrderHistoryModel
{
    [Key]
    [ForeignKey("ManufacturingOrder")]
    public int ManufacturingOrderId { get; set; }

    [Key]
    [ForeignKey("PlantFloorSection")]
    public int PlantFloorSectionId { get; set; }

    public DateTime DateTime { get; set; }

    public string StatusName { get; set; }

    public ManufacturingOrderModel ManufacturingOrder { get; set; }

    public PlantFloorSectionModel PlantFloorSection { get; set; }
}