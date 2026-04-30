using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Service.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Service
{
    public class ItemLocalizationService : IItemLocalizationService
    {
        private readonly IItemLocalizationRepository _repository;

        public ItemLocalizationService(IItemLocalizationRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ItemLocalizationModel> CreateItemLocalizationAsync(ItemLocalizationModel itemLocalization)
        {
            if (itemLocalization == null)
            {
                throw new ArgumentNullException(nameof(itemLocalization), "Item localization cannot be null.");
            }
            
            return await _repository.CreateItemLocalizationAsync(itemLocalization);
        }

        public async Task<List<ItemLocalizationModel>> GetAllItemLocalizationsAsync()
        {
            return await _repository.GetAllItemLocalizationsAsync();
        }

        public async Task<ItemLocalizationModel> GetItemLocalizationByIdAsync(int itemLocalizationId)
        {
            return await _repository.GetItemLocalizationByIdAsync(itemLocalizationId);
        }

        public async Task<ItemLocalizationModel> UpdateItemLocalizationAsync(int itemLocalizationId, DateTime datetime)
        {
            var updatedItem = new ItemLocalizationModel
            {
                ItemLocalizationId = itemLocalizationId,
                DateTime = datetime
            };
            return await _repository.UpdateItemLocalizationAsync(updatedItem);
        }

        public async Task<bool> DeleteItemLocalizationAsync(int itemLocalizationId)
        {
            return await _repository.DeleteItemLocalizationAsync(itemLocalizationId);
        }
    }
}
