using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Repository
{
    public class ItemLocalizationRepository : IItemLocalizationRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemLocalizationRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ItemLocalizationModel> CreateItemLocalizationAsync(ItemLocalizationModel itemLocalization)
        {
            _context.ItemLocalization.Add(itemLocalization);
            await _context.SaveChangesAsync();
            return itemLocalization;
        }

        public async Task<ItemLocalizationModel> GetItemLocalizationByIdAsync(int id)
        {
            return await _context.ItemLocalization 
                .FirstOrDefaultAsync(il => il.ItemLocalizationId == id);
        }

        public async Task<ItemLocalizationModel> UpdateItemLocalizationAsync(ItemLocalizationModel updatedItem)
        {
            var existingItem = await _context.ItemLocalization
                .FirstOrDefaultAsync(il => il.ItemLocalizationId == updatedItem.ItemLocalizationId);

            if (existingItem == null)
            {
                return null;
            }

            existingItem.ItemRawId = updatedItem.ItemRawId;
            existingItem.ContainerLocalizationId = updatedItem.ContainerLocalizationId;
            existingItem.DateTime = updatedItem.DateTime;

            _context.ItemLocalization.Update(existingItem);
            await _context.SaveChangesAsync();

            return existingItem;
        }

        public async Task<bool> DeleteItemLocalizationAsync(int id)
        {
            var existingItem = await _context.ItemLocalization
                .FirstOrDefaultAsync(il => il.ItemLocalizationId == id);

            if (existingItem == null)
            {
                return false;
            }

            _context.ItemLocalization.Remove(existingItem);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<ItemLocalizationModel>> GetAllItemLocalizationsAsync()
        {
            return await _context.ItemLocalization
                .ToListAsync();
        }
    }
}
