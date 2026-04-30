using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO
{
    public class ClientDTO
    {
        [Required] public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string FiscalNumber { get; set; }
    }

    public class CreateClientDTO
    {
        [Required] public string Name { get; set; }
        [Required] public string FiscalNumber { get; set; }
    }

    public class UpdateClientDTO
    {
        [Required] public string Name { get; set; }
        [Required] public string FiscalNumber { get; set; }
    }
    
    public class ClientIdDTO
    {
        [Required] public int Id { get; set; }
    }

    public class UpdateClientWithIdDTO
    {
        [Required] public int Id { get; set; }
        [Required] public string Name { get; set; }
        [Required] public string FiscalNumber { get; set; }
    }
}