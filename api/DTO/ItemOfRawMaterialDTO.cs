using System.Text.Json.Serialization;

public class ItemOfRawMaterialDTO
{
    public int ItemRawId { get; set; }

    public string ItemCode { get; set; }

    public int Quantity { get; set; }

    public string Unit { get; set; }

    public int LotOfRawMaterialId { get; set; }

    public int ManufacturingOrderId { get; set; }

    public int ItemInContainerId { get; set; }

    public int ManufacturingOrderPhaseId { get; set; } 
}