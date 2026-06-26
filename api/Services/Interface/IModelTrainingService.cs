using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IModelTrainingService
{
    Task<MlTrainResultDTO> RunTraining();
}