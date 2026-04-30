using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models
{
    public class LotOfRawMaterialModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int LotId { get; set; }

        [JsonPropertyName("id")]
        [JsonConverter(typeof(PropertyConverter))]
        public string LotCode { get; set; }

        [JsonConverter(typeof(PropertyConverter))]
        public string LotNumber { get; set; }

        public int LotQuantity { get; set; }

        public string LotUnit { get; set; }
        
        
        public int RawMaterialId { get; set; }

        [ForeignKey("RawMaterialId")]
        public RawMaterialModel RawMaterials { get; set; }

        public ICollection<ItemOfRawMaterialModel> ItemOfRawMaterials { get; set; }
        
        public List<int> HistoricalWeights { get; set; } = new List<int>();

    }

}