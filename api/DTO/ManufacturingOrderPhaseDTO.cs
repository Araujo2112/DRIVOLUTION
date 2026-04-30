using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ManufacturingOrderPhaseDTO
{
    [Required] public int Id { get; set; }
    [Required] public string CustomizationParams { get; set; }
    [Required] public int Quantity { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    [Required] public DateTime DateTimeInit { get; set; }
    [Required] public DateTime DateTimeEnd { get; set; }
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }
}

public class CreateManufacturingOrderPhaseDTO
{
    [Required] public string CustomizationParams { get; set; }
    [Required] public int Quantity { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    [Required] public DateTime DateTimeInit { get; set; }
    [Required] public DateTime DateTimeEnd { get; set; }
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }
}

public class UpdateManufacturingOrderPhaseDTO
{
    [Required] public string CustomizationParams { get; set; }
    [Required] public int Quantity { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    [Required] public DateTime DateTimeInit { get; set; }
    [Required] public DateTime DateTimeEnd { get; set; }
    
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }

}

public class ManufacturingOrderPhaseIdDTO
{
    [Required] public int Id { get; set; }
}

public class UpdateManufacturingOrderPhaseWithIdDTO
{
    [Required] public int Id { get; set; }
    [Required] public string CustomizationParams { get; set; }
    [Required] public int Quantity { get; set; }
    [Required] public DateTime SheduleInit { get; set; }
    [Required] public DateTime DateTimeInit { get; set; }
    [Required] public DateTime DateTimeEnd { get; set; }
    
    [Required] public int ManufacturingOrderId { get; set; }
    [Required] public int ManufacturingPhaseId { get; set; }
}