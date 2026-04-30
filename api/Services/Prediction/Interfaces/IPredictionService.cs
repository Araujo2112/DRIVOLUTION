// IPredictionService.cs

using ApiTexPact.DTO;

namespace ApiTexPact.Services.Prediction.Interfaces;

public interface IPredictionService
{
    Task<TrainResultDto> TrainAsync();
    Task<float[]> PredictAsync(List<SinglePredictRequest> features);
    // Mantém os CRUD originais, se ainda precisares:
    Task<PredictionDTO> CreatePredictionAsync(PredictionDTO dto);
    Task<PredictionDTO> GetPredictionByIdAsync(int id);
    Task<List<PredictionDTO>> GetAllPredictionsAsync();
    Task<PredictionDTO> UpdatePredictionAsync(PredictionDTO dto);
    Task<bool> DeletePredictionAsync(int id);
}