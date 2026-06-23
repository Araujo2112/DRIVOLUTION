using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IPhaseSequenceRepository
{
    Task<IEnumerable<PhaseSequenceModel>> GetByModel(int modelId);
    Task<PhaseSequenceModel?> GetById(int id);
    Task<PhaseSequenceModel> Create(PhaseSequenceModel entity);
    Task Update(PhaseSequenceModel entity);
    Task Delete(int id);
}
