namespace Drivolution.DTO.Client;

public class ClientOrderProductDTO
{
    public string Vin { get; set; } = string.Empty;

    public string CurrentPhase { get; set; } = string.Empty;

    public DateTime? EstimatedFinish { get; set; }
}