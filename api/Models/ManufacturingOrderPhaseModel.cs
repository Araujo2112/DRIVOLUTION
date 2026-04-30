using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

public class ManufacturingOrderPhaseModel
{
    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public string CustomizationParams { get; set; }

    public int Quantity { get; set; }

    public DateTime SheduleInit { get; set; }

    public DateTime DateTimeInit { get; set; }

    public DateTime DateTimeEnd { get; set; }

    [ForeignKey("ManufacturingOrder")] 
    public int ManufacturingOrderId { get; set; }

    [ForeignKey("ManufacturingPhase")] 
    public int ManufacturingPhaseId { get; set; }
    
    public ManufacturingOrderModel ManufacturingOrder { get; set; }
    
    public ManufacturingPhaseModel ManufacturingPhase { get; set; }
    
    public ICollection<ItemOfRawMaterialModel> ItemOfRawMaterials { get; set; }
    
    
}