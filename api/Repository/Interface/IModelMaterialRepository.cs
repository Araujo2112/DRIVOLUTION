using Drivolution.Models;
namespace Drivolution.Repository.Interface;
public interface IModelMaterialRepository
{
    Task<IEnumerable<ModelMaterialModel>> GetByModel(int modelId);
    Task<IEnumerable<ModelMaterialModel>> GetByModelAndPhase(int modelId, int phaseId);
    Task<ModelMaterialModel> Create(ModelMaterialModel entity);
    Task Update(ModelMaterialModel entity);
    Task Delete(int id);
}
