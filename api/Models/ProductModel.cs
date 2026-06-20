using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiTexPact.Models;

[Table("product")]
public class ProductModel
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("manufacturing_order_id")]
    [Required]
    public int ManufacturingOrderId { get; set; }

    [Column("model_id")]
    [Required]
    public int ModelId { get; set; }

    [Column("serial_number")]
    public string? SerialNumber { get; set; }

    [Column("lot_number")]
    public string? LotNumber { get; set; }

    [Column("production_date")]
    public DateTime? ProductionDate { get; set; }

    // Navigation
    [ForeignKey("ManufacturingOrderId")]
    public ManufacturingOrderModel ManufacturingOrder { get; set; } = null!;

    [ForeignKey("ModelId")]
    public CarModelModel CarModel { get; set; } = null!;

    public ICollection<ProductPhaseModel> ProductPhases { get; set; } = new List<ProductPhaseModel>();
    public ICollection<QualityCheckModel> QualityChecks { get; set; } = new List<QualityCheckModel>();
    public ICollection<SupportedProductModel> SupportedProducts { get; set; } = new List<SupportedProductModel>();
    public ICollection<ProductConfigModel> ProductConfigs { get; set; } = new List<ProductConfigModel>();

    public ICollection<AlertModel> Alerts { get; set; } = new List<AlertModel>();
}
