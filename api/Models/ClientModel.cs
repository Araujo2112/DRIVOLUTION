using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

public class ClientModel
{
    [Key] 
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int Id { get; set; }

    public string Name { get; set; }

    public string FiscalNumber { get; set; }
    
    public virtual ICollection<ManufacturingOrderModel> ManufacturingOrders { get; set; }

}