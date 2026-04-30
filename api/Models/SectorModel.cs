using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTextPact.Models;

public class SectorModel
{
    [Key]
    public string SectorId { get; set; }
    
    public string SectorName { get; set; }
    
    [ForeignKey("Factory")]
    public string FactoryId { get; set; }
    public FactoryModel Factory { get; set; } 
        
    public ICollection<PorticoModel> Porticos { get; set; }
    public ICollection<ContainerModel> Containers { get; set; }
}