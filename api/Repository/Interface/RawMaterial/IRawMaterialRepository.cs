using ApiTexPact.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Repository.Interface
{
    public interface IRawMaterialRepository
    {
        Task<RawMaterialModel> CreateRawMaterialAsync(RawMaterialModel rawMaterial);
        Task<List<RawMaterialModel>> GetAllRawMaterialsAsync();
        Task<RawMaterialModel?> GetRawMaterialByIdAsync(int id);
        Task<RawMaterialModel> UpdateRawMaterialAsync(int id, RawMaterialModel rawMaterial);
        Task<bool> DeleteRawMaterialByIdAsync(int id);
    }
}