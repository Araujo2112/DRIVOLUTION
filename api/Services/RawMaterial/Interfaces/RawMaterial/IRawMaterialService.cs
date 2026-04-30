using ApiTexPact.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Service.Interface
{
    public interface IRawMaterialService
    {
        Task<RawMaterialModel> CreateRawMaterialAsync(RawMaterialModel rawMaterial);
        Task<List<RawMaterialModel>> GetAllRawMaterialsAsync();
        Task<RawMaterialModel> GetRawMaterialByIdAsync(int id);
        Task<RawMaterialModel> UpdateRawMaterialAsync(int id, RawMaterialModel rawMaterial);
        Task<bool> DeleteRawMaterialByIdAsync(int id);
    }
}