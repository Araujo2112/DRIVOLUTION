using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

public class ManufacturingProcessModel
{
    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    
    public string ProcessName { get; set; }
    
    public string Info { get; set; }
    
    [ForeignKey("Product")] 
    public int ProductId { get; set; }
    
    public ProductModel Product { get; set; }
    
    
    public ICollection<ManufacturingOrderModel> ManufacturingOrders { get; set; }
    
    public ICollection<ManufacturingProcessPhaseModel> ManufacturingProcessPhases { get; set; }

    
}