using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ProductLotDTO
{
    [Required] public int Id { get; set; }
    [Required] public string LotNumber { get; set; }
    [Required] public string LotUnit { get; set; }
    [Required] public int LotQuantity { get; set; }
    [Required] public bool Ready { get; set; }
    
    [Required] public string ProductLotId { get; set; }

    [Required] public string ProductName { get; set; }
    
    [Required] public string Info { get; set; }
    
    [Required] public string ProductId { get; set; }
    
}

public class CreateProductLotDTO
{
    [Required] public string LotNumber { get; set; }
    [Required] public string LotUnit { get; set; }
    [Required] public int LotQuantity { get; set; }
    [Required] public bool Ready { get; set; }
    
    [Required] public string ProductLotId { get; set; }
    [Required] public int ProductId { get; set; }
}


public class ProductLotIdDTO
{
    [Required] public int Id { get; set; }
}

public class UpdateProductLotWithIdDTO
{
    [Required] public string LotUnit { get; set; }
    [Required] public int LotQuantity { get; set; }
    [Required] public bool Ready { get; set; }
    [Required] public int ProductId { get; set; }
}