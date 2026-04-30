using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class ProductModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string Name { get; set; }

    public string Info { get; set; }
    
    
    [JsonPropertyName("ProductId")]
    [JsonConverter(typeof(PropertyConverter))]
    public string ProductId { get; set; }
    
    public ICollection<ManufacturingProcessModel> ManufacturingProcesses { get; set; }
    
    public ICollection<ProductLotModel> ProductLots { get; set; }
    
}