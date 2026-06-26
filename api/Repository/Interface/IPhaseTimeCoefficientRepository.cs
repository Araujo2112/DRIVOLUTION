using Drivolution.Models;
namespace Drivolution.Repository.Interface;

public interface IPhaseTimeCoefficientRepository
{
    Task<IEnumerable<PhaseTimeCoefficientModel>> GetAll();
    Task<DateTime?> GetLastTrainedAt();
}