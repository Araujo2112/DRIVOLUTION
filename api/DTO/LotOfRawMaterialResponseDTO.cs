namespace ApiTexPact.DTO;

public class LotOfRawMaterialResponseDTO
{
    public int LotId { get; set; }
    public string LotNumber { get; set; }
    public int LotQuantity { get; set; }
    public string LotUnit { get; set; }
    
    public int RawMaterialId { get; set; }
    public string RawMaterialName { get; set; }
        
    public List<int> HistoricalWeights { get; set; } = new List<int>();
}
