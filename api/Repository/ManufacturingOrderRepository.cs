using ApiTexPact.Data;
using Microsoft.EntityFrameworkCore;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.ManufacturingOrder;

namespace ApiTexPact.Repository
{
    public class ManufacturingOrderRepository : IManufacturingOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public ManufacturingOrderRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ManufacturingOrderModel>> GetAll()
        {
            return await _context.ManufacturingOrders.ToListAsync();
        }

        public async Task<ManufacturingOrderModel> GetById(int id)
        {
            return await _context.ManufacturingOrders.FindAsync(id);
        }

        public async Task<ManufacturingOrderModel> Create(ManufacturingOrderModel manufacturingOrder)
        {
            _context.ManufacturingOrders.Add(manufacturingOrder);
            await _context.SaveChangesAsync();
            return manufacturingOrder;
        }

        public async Task Update(ManufacturingOrderModel manufacturingOrder)
        {
            _context.ManufacturingOrders.Update(manufacturingOrder);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var order = await _context.ManufacturingOrders.FindAsync(id);
            if (order != null)
            {
                _context.ManufacturingOrders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.ManufacturingOrders.AnyAsync(o => o.Id == id);
        }
        
        public async Task<ManufacturingOrderModel?> GetByIdWithDetailsForGraphAsync(int manufacturingOrderId)
        {
            _context.ChangeTracker.Clear();
    
            var result = await _context.ManufacturingOrders
                .Include(o => o.Client)
                .Include(o => o.ProductLot)
                .Include(o => o.ManufacturingProcess)
                .ThenInclude(mp => mp.ManufacturingProcessPhases)
                .ThenInclude(mpp => mpp.ManufacturingPhase)
                .ThenInclude(mp => mp.PlantFloorSection)
                .Include(o => o.ManufacturingOrderHistory)
                .Include(o => o.ManufacturingOrderPhases)
                .ThenInclude(mop => mop.ManufacturingPhase)
                .ThenInclude(mp => mp.PlantFloorSection)
                
                .Include(o => o.ItemsOfRawMaterial)
                .ThenInclude(item => item.LotOfRawMaterial)
                .ThenInclude(lot => lot.RawMaterials)
                
                .Include(o => o.ItemsOfRawMaterial)
                .ThenInclude(item => item.ItemInContainer)
                .ThenInclude(iic => iic.Container)
                .ThenInclude(c => c.LocalizationHistories)
                
                .Include(o => o.ItemsOfRawMaterial)
                .ThenInclude(item => item.ItemLocalizations)
        
                .AsSplitQuery()
                .FirstOrDefaultAsync(o => o.Id == manufacturingOrderId);

            return result;
        }



    }
}
