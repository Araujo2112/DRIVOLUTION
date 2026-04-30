using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ManufacturingOrderDTO
{
    [Required] public int Id { get; set; }
    [Required] public int OrderNumber { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    public string Observations { get; set; }

    [Required] public string ManufacturingOrderId { get; set; }
    [Required] public int ClientId { get; set; }
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ProductLotId { get; set; }
}

public class CreateManufacturingOrderDTO
{
    [Required] public int OrderNumber { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    public string Observations { get; set; }
    
    public string ManufacturingOrderId { get; set; }

    [Required] public int ClientId { get; set; }
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ProductLotId { get; set; }
}

public class UpdateManufacturingOrderDTO
{
    [Required] public int OrderNumber { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    public string Observations { get; set; }

    [Required] public int ClientId { get; set; }
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ProductLotId { get; set; }
}

public class ManufacturingOrderIdDTO
{
    [Required] public int Id { get; set; }
}

public class UpdateManufacturingOrderWithIdDTO
{
    [Required] public int Id { get; set; }

    [Required] public int OrderNumber { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    public string Observations { get; set; }

    [Required] public int ClientId { get; set; }
    [Required] public int ManufacturingProcessId { get; set; }
    [Required] public int ProductLotId { get; set; }
}