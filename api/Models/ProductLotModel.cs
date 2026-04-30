using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class ProductLotModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string LotNumber { get; set; }

    public string LotUnit { get; set; }

    public int LotQuantity { get; set; }

    public bool Ready { get; set; }


    [JsonPropertyName("ProductLotId")]
    [JsonConverter(typeof(PropertyConverter))]
    public string ProductLotId { get; set; }

    [ForeignKey("Product")] public int ProductId { get; set; }


    public ProductModel Product { get; set; }
    
    
    public ICollection<ManufacturingOrderModel> ManufacturingOrders { get; set; }
    
}