namespace ApiTexPact.Services.ArrowFlightClient.Interfaces;

public interface IQuicPredictionClientService
{
    Task<byte[]> SendTrainDataAsync(
        byte[] onnx,
        int version,
        DateTime lastDate,
        string modelType,
        List<Dictionary<string, object>> data);


    Task<float[]> GetPredictionsAsync(
        byte[] modelBytes,
        List<Dictionary<string, object>> features);
}