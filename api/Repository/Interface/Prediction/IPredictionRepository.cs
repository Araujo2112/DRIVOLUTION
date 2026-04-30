using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface.Prediction;

public interface IPredictionRepository
{
    Task<PredictionModel> CreateAsync(PredictionModel model);
    Task<PredictionModel?> GetByIdAsync(int id);
    Task<List<PredictionModel>> GetAllAsync();
    Task<PredictionModel> UpdateAsync(PredictionModel model);
    Task<bool> DeleteAsync(int id);
    Task<List<Dictionary<string, object>>> GetTrainingDatasetAsync(DateTime cutoffUtc);
    
    Task<PredictionModel> GetLatestModelAsync();
    
}