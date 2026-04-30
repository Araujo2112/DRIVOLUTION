using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class ContainerModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int ContainerId { get; set; }

    [JsonPropertyName("id")]
    [JsonConverter(typeof(PropertyConverter))] 
    public string ContainerCode { get; set; }

    [JsonConverter(typeof(PropertyConverter))] 
    public string ContainerName { get; set; }

    public float ContainerVolume { get; set; }

    public bool Activate { get; set; }

 
    public ICollection<ContainerLocalizationModel> LocalizationHistories { get; set; }
    
    public ICollection<ItemInContainerModel> IteminContainers { get; set; }
}