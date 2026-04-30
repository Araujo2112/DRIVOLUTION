using ApiTexPact.Models;
using ApiTexPact.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Service.Interface
{
    public interface IItemOfRawMaterialService
    {
        Task<List<ItemOfRawMaterialModel>> GetAllAsync();
        Task<ItemOfRawMaterialModel> GetByCodeAsync(int itemRawId);
        Task<ItemOfRawMaterialModel> CreateAsync(ItemOfRawMaterialDTO itemDto);
        Task<ItemOfRawMaterialModel> UpdateAsync(ItemOfRawMaterialDTO itemDto);
        Task<bool> DeleteByCodeAsync(int itemRawId);
    }
}