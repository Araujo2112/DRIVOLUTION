using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class CheckpointModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int CheckpointId { get; set; }

    [JsonPropertyName("id")]
    public string CheckpointCode { get; set; } 
    
    [JsonConverter(typeof(PropertyConverter))] 
    public string Name { get; set; }
    
    public bool Status { get; set; }
    
    [ForeignKey("PlantFloorSection")]
    public int SectionId { get; set; } 
    
    public PlantFloorSectionModel PlantFloorSection { get; set; }
}

