using ApiTexPact.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Repository.Interface
{
    public interface IItemOfRawMaterialRepository
    {
        Task<List<ItemOfRawMaterialModel>> GetAllAsync();
        Task<ItemOfRawMaterialModel> GetByCodeAsync(int ItemRawId);
        Task<ItemOfRawMaterialModel> CreateAsync(ItemOfRawMaterialDTO itemDto);
        Task<ItemOfRawMaterialModel> UpdateAsync(ItemOfRawMaterialDTO itemDto);

        Task<bool> DeleteByCodeAsync(int ItemRawId);
    }
}