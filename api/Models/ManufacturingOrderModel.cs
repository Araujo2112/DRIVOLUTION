using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class ManufacturingOrderModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public int OrderNumber { get; set; }
    
    public DateTime SheduleInit { get; set; }
    
    public string Observations { get; set; }
    
    
    [JsonPropertyName("ManufacturingPhaseId")]
    [JsonConverter(typeof(PropertyConverter))]
    public string ManufacturingOrderId { get; set; }

    [ForeignKey("Client")]
    public int ClientId { get; set; }
    
    [ForeignKey("ManufacturingProcess")]
    public int ManufacturingProcessId { get; set; }
    
    [ForeignKey("ProductLot")]
    public int ProductLotId { get; set; }
    
    
    public ClientModel Client { get; set; }
    
    public ManufacturingProcessModel ManufacturingProcess { get; set; }
    
    public ProductLotModel ProductLot { get; set; }
    
    
    public ICollection<ManufacturingOrderHistoryModel> ManufacturingOrderHistory { get; set; }
    public ICollection<ManufacturingOrderPhaseModel> ManufacturingOrderPhases { get; set; }
    
    public ICollection<ItemOfRawMaterialModel> ItemsOfRawMaterial { get; set; }


    
}