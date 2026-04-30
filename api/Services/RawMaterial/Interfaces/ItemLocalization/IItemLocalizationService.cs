using ApiTexPact.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Service.Interface
{
    public interface IItemLocalizationService
    {
        Task<ItemLocalizationModel> CreateItemLocalizationAsync(ItemLocalizationModel itemLocalization);
        Task<List<ItemLocalizationModel>> GetAllItemLocalizationsAsync();


        Task<ItemLocalizationModel> GetItemLocalizationByIdAsync(int itemLocalizationId);

        Task<ItemLocalizationModel> UpdateItemLocalizationAsync(int itemLocalizationId, DateTime datetime);

        Task<bool> DeleteItemLocalizationAsync(int itemLocalizationId);
    }
}