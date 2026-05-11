using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("product_config")]
public class ProductConfigModel {
    [Key, Column("id")] 
    public int Id { get; set; }
    [Column("product_id")] 
    public int ProductId { get; set; }
    [Column("config_option_id")] 
    public int ConfigOptionId { get; set; }

    [ForeignKey("ProductId")] 
    public ProductModel Product { get; set; } = null!;
    [ForeignKey("ConfigOptionId")] 
    public ConfigOptionModel ConfigOption { get; set; } = null!;
}