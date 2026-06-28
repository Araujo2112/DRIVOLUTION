using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Drivolution.Models;

[Table("client_order")]
public class ClientOrderModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("order_number")]
    [Required]
    public string OrderNumber { get; set; } = string.Empty;

    [Column("order_date")]
    [Required]
    public DateTime OrderDate { get; set; }

    [Column("customer_name")]
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Column("quantity")]
    [Required]
    public int Quantity { get; set; } = 1;

    [Column("app_user_id")]
    public int? AppUserId { get; set; }

    // Navigation
    public UserModel? AppUser { get; set; }

    public ICollection<ManufacturingOrderModel> ManufacturingOrders { get; set; } = new List<ManufacturingOrderModel>();
}