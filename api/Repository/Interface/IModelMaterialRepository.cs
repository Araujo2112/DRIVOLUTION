using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface.ModelMaterial;
public interface IModelMaterialRepository
{
    Task<IEnumerable<ModelMaterialModel>> GetByModel(int modelId);
    Task<IEnumerable<ModelMaterialModel>> GetByModelAndPhase(int modelId, int phaseId);
    Task<ModelMaterialModel> Create(ModelMaterialModel entity);
    Task Update(ModelMaterialModel entity);
    Task Delete(int id);
}
