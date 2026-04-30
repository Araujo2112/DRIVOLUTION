using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiTexPact.Data;

namespace ApiTexPact.Repository
{
    public class ItemOfRawMaterialRepository : IItemOfRawMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        public ItemOfRawMaterialRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ItemOfRawMaterialModel>> GetAllAsync() 
        {
            return await _context.ItemOfRawMaterial
                .ToListAsync();
        }

        public async Task<ItemOfRawMaterialModel> GetByCodeAsync(int itemRawId) 
        {
            return await _context.ItemOfRawMaterial
                .FirstOrDefaultAsync(i => i.ItemRawId == itemRawId);
        }
        
        
        public async Task<ItemOfRawMaterialModel> CreateAsync(ItemOfRawMaterialDTO itemDto)
        {
            var item = new ItemOfRawMaterialModel
            {
                ItemRawId = itemDto.ItemRawId,
                ItemCode = itemDto.ItemCode,
                Quantity = itemDto.Quantity,
                Unit = itemDto.Unit,
                LotOfRawMaterialId = itemDto.LotOfRawMaterialId,
                ItemInContainerId = itemDto.ItemInContainerId,
                ManufacturingOrderId = itemDto.ManufacturingOrderId,
                ManufacturingOrderPhaseId = itemDto.ManufacturingOrderPhaseId 
            };

            await _context.ItemOfRawMaterial.AddAsync(item);
            await _context.SaveChangesAsync();
            return item;
        }

        public async Task<ItemOfRawMaterialModel> UpdateAsync(ItemOfRawMaterialDTO itemDto)
        {
            var item = await _context.ItemOfRawMaterial.FirstOrDefaultAsync(i => i.ItemRawId == itemDto.ItemRawId);
            if (item == null)
                return null;
            
            item.Quantity = itemDto.Quantity;
            item.Unit = itemDto.Unit;
            item.LotOfRawMaterialId = itemDto.LotOfRawMaterialId;
            item.ManufacturingOrderId = itemDto.ManufacturingOrderId;
            item.ManufacturingOrderPhaseId = itemDto.ManufacturingOrderPhaseId; 

            _context.ItemOfRawMaterial.Update(item);
            await _context.SaveChangesAsync();
            return item;
        }


        public async Task<bool> DeleteByCodeAsync(int itemRawId) 
        {
            var item = await _context.ItemOfRawMaterial.FirstOrDefaultAsync(i => i.ItemRawId == itemRawId);
            if (item == null)
            {
                return false;
            }

            _context.ItemOfRawMaterial.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
