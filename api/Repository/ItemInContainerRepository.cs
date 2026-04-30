using ApiTexPact.Data;
using ApiTexPact.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiTexPact.Repository.Interface;

namespace ApiTexPact.Repository
{
    public class ItemInContainerRepository : IItemInContainerRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemInContainerRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ItemInContainerModel> AddItemToContainerAsync(ItemInContainerModel item)
        {
            var existingItem = await _context.ItemInContainer
                .FirstOrDefaultAsync(i => i.ItemInContainerId == item.ItemInContainerId);

            if (existingItem != null)
            {
                throw new Exception($"Item with code {item.ItemInContainerId} already exists.");
            }

            _context.ItemInContainer.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }

        public async Task<List<ItemInContainerModel>> GetAllItemInContainerAsync()
        {
            return await _context.ItemInContainer.ToListAsync();
        }

        public async Task<ItemInContainerModel> GetItemAsync(int itemInContainerId)
        {
            if (itemInContainerId == 0)
            {
                throw new ArgumentException("The item code cannot be null.", nameof(itemInContainerId));
            }

            var item = await _context.ItemInContainer
                .FirstOrDefaultAsync(i => i.ItemInContainerId == itemInContainerId);

            return item ?? throw new Exception($"ItemInContainer with code {itemInContainerId} not found.");
        }

        public async Task<bool> RemoveItemFromContainerAsync(int itemInContainerId)
        {
            var item = await _context.ItemInContainer
                .FirstOrDefaultAsync(i => i.ItemInContainerId == itemInContainerId);

            if (item == null)
            {
                return false;
            }

            _context.ItemInContainer.Remove(item);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ItemInContainerModel> UpdateItemInContainerAsync(ItemInContainerModel item)
        {
            var existingItem = await _context.ItemInContainer
                .FirstOrDefaultAsync(i => i.ItemInContainerId == item.ItemInContainerId && i.ContainerId == item.ContainerId);

            if (existingItem == null)
            {
                throw new Exception($"Item with code {item.ItemInContainerId} not found in container {item.ContainerId}.");
            }

            existingItem.ItemInContainerId = item.ItemInContainerId;
            existingItem.DateTimeIn = item.DateTimeIn;
            existingItem.DateTimeOut = item.DateTimeOut;

            _context.ItemInContainer.Update(existingItem);
            await _context.SaveChangesAsync();

            return existingItem;
        }
    }
}
