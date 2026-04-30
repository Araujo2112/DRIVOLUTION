using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class PlantFloorSectionModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int SectionId { get; set; }

    [JsonPropertyName("id")]
    [JsonConverter(typeof(PropertyConverter))]
    public string SectionCode { get; set; } 
    
    public string name { get; set; }
    
    //Relation 1:1 to the table ManufacturingPhase (The FK is in the other side in the other table)
    public ManufacturingPhaseModel ManufacturingPhase { get; set; }
    public ICollection<ContainerLocalizationModel> LocalizationHistories { get; set; }
    
    public ICollection<CheckpointModel> Checkpoints { get; set; }
    
    public ICollection<ManufacturingOrderHistoryModel> OrderHistory { get; set; }
    
    
}