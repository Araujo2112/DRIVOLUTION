using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ApiTexPact.Models;

public class ContainerLocalizationModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Column(Order = 1)] public int ContainerId { get; set; }

    [ForeignKey(nameof(ContainerId))] public ContainerModel Container { get; set; }

    [Column(Order = 2)] public int SectionId { get; set; }

    [ForeignKey(nameof(SectionId))] public PlantFloorSectionModel PlantFloorSection { get; set; }

    [JsonPropertyName("datetime")] public DateTime Datetime { get; set; }

    public ICollection<ItemLocalizationModel> ItemLocalizations { get; set; }
}