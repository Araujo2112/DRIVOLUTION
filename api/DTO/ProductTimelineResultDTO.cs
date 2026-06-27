namespace Drivolution.DTO;

public class ProductTimelineResultDTO
{
    public int ProductId { get; set; }
    public int ModelId { get; set; }
    public string? SerialNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ProductTimelineDTO> Phases { get; set; } = new();
}