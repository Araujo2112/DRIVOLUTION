using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models
{
    public class ItemLocalizationModel
    {
        [Key]
        public int ItemLocalizationId { get; set; }  

        
        public int ItemRawId { get; set; } 
        
        [ForeignKey(nameof(ItemRawId))]
        public virtual ItemOfRawMaterialModel ItemOfRawMaterial { get; set; }
        
        public int ContainerLocalizationId { get; set; }
        
        [ForeignKey(nameof(ContainerLocalizationId))]
        public ContainerLocalizationModel ContainerLocalization { get; set; }


        public DateTime DateTime { get; set; }
    }
}