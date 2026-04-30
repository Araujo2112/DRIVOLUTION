using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ApiTexPact.DTO
{
    public class LotOfRawMaterialDTO
    {
        [Required] public int LotId { get; set; }
        
        [Required] public string LotCode { get; set; }

        [Required] public string LotNumber { get; set; }

        [Required] public int LotQuantity { get; set; }

        [Required] public string LotUnit { get; set; }

        [Required] public int RawMaterialId { get; set; }
        
    }
}