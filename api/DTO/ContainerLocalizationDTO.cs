using System.Text.Json.Serialization;
using ApiTexPact.Converters;
using Newtonsoft.Json.Converters;

namespace ApiTexPact.DTO;

public class ContainerLocalizationDTO
{
    public int Id { get; set; }
    public int ContainerId { get; set; } 
    public int SectionId { get; set; }
    
    public DateTime datetime { get; set; }
}