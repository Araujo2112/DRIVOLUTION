using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiTexPact.Repository
{
    public class RawMaterialRepository : IRawMaterialRepository
    {
        private readonly ApplicationDbContext _context;

        public RawMaterialRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<RawMaterialModel>> GetAllRawMaterialsAsync()
        {
            return await _context.RawMaterial.ToListAsync();
        }

        public async Task<RawMaterialModel?> GetRawMaterialByIdAsync(int id)
        {
            return await _context.RawMaterial
                .FirstOrDefaultAsync(rm => rm.RawId == id);
        }

        public async Task<RawMaterialModel> CreateRawMaterialAsync(RawMaterialModel rawMaterial)
        {
            _context.RawMaterial.Add(rawMaterial);
            await _context.SaveChangesAsync();
            return rawMaterial;
        }

        public async Task<RawMaterialModel> UpdateRawMaterialAsync(int id, RawMaterialModel rawMaterial)
        {
            var existingRawMaterial = await _context.RawMaterial
                .FirstOrDefaultAsync(rm => rm.RawId == id);

            if (existingRawMaterial == null)
            {
                throw new Exception($"RawMaterial with ID {id} not found.");
            }
            
            existingRawMaterial.Name = rawMaterial.Name;
            existingRawMaterial.Info = rawMaterial.Info;

            _context.RawMaterial.Update(existingRawMaterial);
            await _context.SaveChangesAsync();

            return existingRawMaterial; 
        }

        public async Task<bool> DeleteRawMaterialByIdAsync(int id)
        {
            var rawMaterial = await _context.RawMaterial
                .FirstOrDefaultAsync(rm => rm.RawId == id);

            if (rawMaterial == null)
            {
                return false;
            }

            _context.RawMaterial.Remove(rawMaterial);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
