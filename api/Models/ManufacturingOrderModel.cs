using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("manufacturing_order")]
public class ManufacturingOrderModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("client_order_id")]
    [Required]
    public int ClientOrderId { get; set; }

    [Column("manufacturing_order_number")]
    [Required]
    public string ManufacturingOrderNumber { get; set; } = string.Empty;

    [Column("start_date")]
    [Required]
    public DateTime StartDate { get; set; }

    [Column("end_date")]
    public DateTime? EndDate { get; set; }

    [Column("status")]
    public string? Status { get; set; }

    // Navigation
    [ForeignKey("ClientOrderId")]
    public ClientOrderModel ClientOrder { get; set; } = null!;

    public ICollection<ProductModel> Products { get; set; } = new List<ProductModel>();
}
