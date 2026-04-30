using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ManufacturingProcessDTO
{
    [Required] public int Id { get; set; }
    [Required] public string ProcessName { get; set; }
    [Required] public string Info { get; set; }
    [Required] public int ProductId { get; set; }
    [Required] public string ProductName { get; set; }
}

public class CreateManufacturingProcessDTO
{
    [Required] public string ProcessName { get; set; }
    [Required] public string Info { get; set; }
    [Required] public int ProductId { get; set; }
}

public class UpdateManufacturingProcessDTO
{
    [Required] public string ProcessName { get; set; }
    [Required] public string Info { get; set; }
}

public class ManufacturingProcessIdDTO
{
    [Required] public int Id { get; set; }
}

public class UpdateManufacturingProcessWithIdDTO
{
    [Required] public int Id { get; set; }
    [Required] public string ProcessName { get; set; }
    [Required] public string Info { get; set; }
}