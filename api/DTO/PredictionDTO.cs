using System.Text.Json.Serialization;

namespace ApiTexPact.DTO;

public class PredictionDTO
{
    public int Id { get; set; }
    public byte[] Model { get; set; }
    public DateTime LastDate { get; set; }
    public int ModelVersion { get; set; }
    public string ModelType { get; set; }
}

public class TrainRequestDto
{
    public byte[] ModelOnnx { get; set; }
    public int ModelVersion { get; set; }
    public DateTime LastDate { get; set; }
    public string ModelType { get; set; }
    public List<Dictionary<string, object>> Dataset { get; set; }
}

public class TrainResultDto
{
    public string Message { get; set; }
    public int Version { get; set; }
    public DateTime TrainedUntil { get; set; }
}

public class PredictRequestDto
{
    [JsonPropertyName("features")] public List<SinglePredictRequest> Features { get; set; }
}

public class SinglePredictRequest
{
    [JsonPropertyName("Quantity")] public double Quantity { get; set; }

    [JsonPropertyName("PhaseId")] public int PhaseId { get; set; }

    [JsonPropertyName("SectionId")] public int SectionId { get; set; }

    [JsonPropertyName("ProductId")] public int ProductId { get; set; }

    [JsonPropertyName("ClientId")] public int ClientId { get; set; }

    [JsonPropertyName("LotQty")] public int LotQty { get; set; }

    [JsonPropertyName("Hour")] public int Hour { get; set; }

    [JsonPropertyName("WeekDay")] public int WeekDay { get; set; }
}

public class PredictResponseDto
{
    [JsonPropertyName("predictions")] public List<double> Predictions { get; set; }
}