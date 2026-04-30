using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTextPact.Models;

public class PorticoModel
{
    [Key]
    public string PorticoId { get; set; }
    
    
    public string PorticoName { get; set; }
    
    [ForeignKey("Sector")]
    public string SectorId { get; set; }
    public SectorModel Sector { get; set; }

    public ICollection<MotionSensorModel> MotionSensor { get; set; }

}