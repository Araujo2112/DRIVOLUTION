namespace Drivolution.DTO.Client;

public class ClientOrderSummaryDTO
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public int Quantity { get; set; }

    public int CompletedProducts { get; set; }

    public string Status { get; set; } = string.Empty;
}