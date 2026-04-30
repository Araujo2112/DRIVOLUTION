using System.ComponentModel.DataAnnotations;

namespace ApiTexPact.DTO;

public class ProductDTO
{
    [Required] public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Info { get; set; }
    
    [Required] public string ProductId { get; set; }
}

public class CreateProductDTO
{
    [Required] public string Name { get; set; }
    [Required] public string Info { get; set; }
    
    [Required] public string ProductId { get; set; }
}

public class UpdateProductDTO
{
    [Required] public string Name { get; set; }
    [Required] public string Info { get; set; }
    
}

public class ProductIdDTO
{
    [Required] public int Id { get; set; }
}


public class UpdateProductWithIdDTO
{
    [Required] public int Id { get; set; }
    [Required] public string Name { get; set; }
    [Required] public string Info { get; set; }
    
}