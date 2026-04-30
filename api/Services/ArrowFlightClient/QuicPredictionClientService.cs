using System.Net;
using ApiTexPact.Services.ArrowFlightClient.Interfaces;

namespace ApiTexPact.Services.ArrowFlightClient;

public class QuicPredictionClientService : IQuicPredictionClientService
{
    private readonly HttpClient _http;

    public QuicPredictionClientService(HttpClient http)
    {
        _http = http;
        // reforça que o HttpClient sempre tentará HTTP/3 quando possível
        _http.DefaultRequestVersion = HttpVersion.Version30;
        _http.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
    }

    public async Task<byte[]> SendTrainDataAsync(
        byte[] currentModel,
        int nextVersion,
        DateTime lastDateUtc,
        string modelType,
        List<Dictionary<string, object>> dataset)
    {
        var req = new TrainRequest(
            Convert.ToBase64String(currentModel),
            nextVersion,
            lastDateUtc.ToString("o"),
            modelType,
            dataset
        );

        // serializa o JSON do body
        var content = JsonContent.Create(req);

        HttpResponseMessage resp;
        try
        {
            // 1) tenta postar via HTTP/3 (QUIC)
            var msgH3 = new HttpRequestMessage(HttpMethod.Post, "train")
            {
                Version = HttpVersion.Version30,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
                Content = content
            };
            resp = await _http.SendAsync(msgH3);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("HTTP/3"))
        {
            // 2) se falhar, faz fallback para HTTP/2
            var msgH2 = new HttpRequestMessage(HttpMethod.Post, "train")
            {
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
                Content = content
            };
            resp = await _http.SendAsync(msgH2);
        }

        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadFromJsonAsync<TrainResponse>();
        if (body is null)
            throw new InvalidOperationException("Resposta vazia do /train");

        return Convert.FromBase64String(body.model_base64);
    }

    public async Task<float[]> GetPredictionsAsync(
        byte[] modelBytes,
        List<Dictionary<string, object>> features)
    {
        var req = new PredictRequest(
            Convert.ToBase64String(modelBytes),
            features
        );
        var content = JsonContent.Create(req);

        HttpResponseMessage resp;
        try
        {
            var msgH3 = new HttpRequestMessage(HttpMethod.Post, "predict")
            {
                Version = HttpVersion.Version30,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
                Content = content
            };
            resp = await _http.SendAsync(msgH3);
        }
        catch (HttpRequestException ex) when (ex.Message.Contains("HTTP/3"))
        {
            var msgH2 = new HttpRequestMessage(HttpMethod.Post, "predict")
            {
                Version = HttpVersion.Version20,
                VersionPolicy = HttpVersionPolicy.RequestVersionOrLower,
                Content = content
            };
            resp = await _http.SendAsync(msgH2);
        }

        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<PredictResponse>()
                   ?? throw new InvalidOperationException("Resposta vazia do /predict");
        return body.Predictions;
    }


    // DTOs internos para serialização
    private record TrainRequest(
        string model_base64,
        int version,
        string last_date_iso,
        string model_type,
        List<Dictionary<string, object>> dataset
    );

    private record TrainResponse(string model_base64);

    private record PredictRequest(
        string ModelBase64,
        List<Dictionary<string, object>> Features
    );

    private record PredictResponse(float[] Predictions);
}