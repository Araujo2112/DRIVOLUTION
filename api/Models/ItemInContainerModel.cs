using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models
{
    public class ItemInContainerModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ItemInContainerId { get; set; }
        
        [JsonConverter(typeof(PropertyConverter))]
        public string ItemCode { get; set; }

        public int ContainerId { get; set; }
        public ContainerModel Container { get; set; }

        public virtual ICollection<ItemOfRawMaterialModel> ItemsOfRawMaterial { get; set; } = new List<ItemOfRawMaterialModel>();

        public DateTime DateTimeIn { get; set; }

        public DateTime DateTimeOut { get; set; }
    }
}