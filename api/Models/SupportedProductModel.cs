using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("supported_product")]
public class SupportedProductModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("support_id")]
    [Required]
    public int SupportId { get; set; }

    [Column("product_id")]
    public int? ProductId { get; set; }

    [Column("datetime_ini")]
    [Required]
    public DateTime DatetimeIni { get; set; }

    [Column("datetime_end")]
    public DateTime? DatetimeEnd { get; set; }

    // Navigation
    [ForeignKey("SupportId")]
    public SupportModel Support { get; set; } = null!;

    [ForeignKey("ProductId")]
    public ProductModel? Product { get; set; }
}
