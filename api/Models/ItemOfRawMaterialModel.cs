using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class ItemOfRawMaterialModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ItemRawId { get; set; }

    [JsonPropertyName("code")]
    [JsonConverter(typeof(PropertyConverter))]
    public string ItemCode { get; set; }

    public int Quantity { get; set; }

    public string Unit { get; set; }

    public int LotOfRawMaterialId { get; set; }

    [ForeignKey(nameof(LotOfRawMaterialId))]
    public virtual LotOfRawMaterialModel LotOfRawMaterial { get; set; }

    public int ItemInContainerId { get; set; }

    [ForeignKey(nameof(ItemInContainerId))]
    public virtual ItemInContainerModel ItemInContainer { get; set; }

    public int ManufacturingOrderPhaseId { get; set; }

    [ForeignKey(nameof(ManufacturingOrderPhaseId))]
    public virtual ManufacturingOrderPhaseModel ManufacturingOrderPhase { get; set; }


    public int ManufacturingOrderId { get; set; }

    [ForeignKey(nameof(ManufacturingOrderId))]
    public virtual ManufacturingOrderModel ManufacturingOrder { get; set; }


    public virtual ICollection<ItemLocalizationModel> ItemLocalizations { get; set; }
        = new List<ItemLocalizationModel>();
}