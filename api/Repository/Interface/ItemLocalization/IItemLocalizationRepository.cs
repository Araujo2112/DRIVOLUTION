using ApiTexPact.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Repository.Interface
{
    public interface IItemLocalizationRepository
    {
        Task<ItemLocalizationModel> CreateItemLocalizationAsync(ItemLocalizationModel itemLocalization);
        

        Task<ItemLocalizationModel> GetItemLocalizationByIdAsync(int itemLocalizationId);

        Task<ItemLocalizationModel> UpdateItemLocalizationAsync(ItemLocalizationModel updatedItem);
        

        Task<bool> DeleteItemLocalizationAsync(int itemLocalizationId);

        Task<List<ItemLocalizationModel>> GetAllItemLocalizationsAsync();
    }
}