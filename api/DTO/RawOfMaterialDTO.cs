using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO
{
    public class RawMaterialDTO
    {
        [Required]
        public int Id { get; set; }  

        [Required]
        public string Name { get; set; }

        public string Info { get; set; }
    }
}