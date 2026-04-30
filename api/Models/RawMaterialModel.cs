using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

public class RawMaterialModel

{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
    public int RawId { get; set; } 

    public string Name { get; set; } 
    
    public string Info { get; set; }  
    
    public virtual ICollection<LotOfRawMaterialModel> LotOfRawMaterials { get; set; }
    
}
