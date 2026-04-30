using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository
{
    public class LotOfRawMaterialRepository : ILotOfRawMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        public LotOfRawMaterialRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<LotOfRawMaterialModel> CreatelotAsync(LotOfRawMaterialModel lot)
        {
            lot.HistoricalWeights = lot.HistoricalWeights ?? new List<int>();
            lot.HistoricalWeights.Add(lot.LotQuantity);

            var existinglot = await _context.LotOfRawMaterial
                .FirstOrDefaultAsync(i => i.LotId == lot.LotId);

            if (existinglot != null)
            {
                throw new Exception($"Lot with code {lot.LotId} already exists.");
            }
            
            _context.LotOfRawMaterial.Add(lot);
            await _context.SaveChangesAsync();

            return lot;
        }

        public async Task<List<LotOfRawMaterialModel>> GetAlllotsAsync()
        {
            return await _context.LotOfRawMaterial.ToListAsync();
        }

        public async Task<LotOfRawMaterialModel> GetlotByCodeAsync(int lotId)
        {
            return await _context.LotOfRawMaterial
                .FirstOrDefaultAsync(l => l.LotId == lotId);
        }

        public async Task<bool> DeletelotByCodeAsync(int lotId)
        {
            var lot = await _context.LotOfRawMaterial
                .FirstOrDefaultAsync(i => i.LotId == lotId);

            if (lot == null)
            {
                return false;
            }

            _context.LotOfRawMaterial.Remove(lot);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<LotOfRawMaterialModel> UpdatelotAsync(
            int lotId,
            string lotCode,
            string lotNumber,
            int lotQuantity,
            string lotUnit)
        {
            var lot = await _context.LotOfRawMaterial
                .FirstOrDefaultAsync(i => i.LotId == lotId);

            if (lot == null)
            {
                throw new Exception($"Lot with code {lotId} not found.");
            }
            
            lot.LotNumber = lotNumber;
            lot.LotQuantity = lotQuantity;
            lot.LotUnit = lotUnit;
            lot.HistoricalWeights.Add(lotQuantity); 

            _context.LotOfRawMaterial.Update(lot);
            await _context.SaveChangesAsync();

            return lot;
        }
    }
}
