using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using ApiTexPact.Converters;

namespace ApiTexPact.Models;

public class ManufacturingPhaseModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string PhaseInfo { get; set; }
    
    public int PhaseDuration { get; set; }
    
    
    [JsonPropertyName("ManufacturingPhaseId")]
    [JsonConverter(typeof(PropertyConverter))]
    public string ManufacturingPhaseId { get; set; }

    [ForeignKey("PlantFloorSection")]
    public int PlantFloorSectionId { get; set; }
    
    
    public PlantFloorSectionModel PlantFloorSection { get; set; }
    
    
    public ICollection<ManufacturingOrderPhaseModel> ManufacturingOrderPhases { get; set; }
    
    public ICollection<ManufacturingProcessPhaseModel> ManufacturingProcessPhases { get; set; }




}