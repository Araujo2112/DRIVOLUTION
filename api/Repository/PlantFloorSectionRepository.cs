using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApiTexPact.Data;

namespace ApiTexPact.Repository
{
    public class PlantFloorSectionRepository : IPlantFloorSectionRepository
    {
        private readonly ApplicationDbContext _context;

        public PlantFloorSectionRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PlantFloorSectionModel> CreatePlantFloorSectionAsync(PlantFloorSectionModel plantFloorSection)
        {
            var existingSection = await _context.PlantFloorSection
                .FirstOrDefaultAsync(s => s.SectionId == plantFloorSection.SectionId);

            if (existingSection != null)
            {
                throw new Exception($"Section with code {plantFloorSection.SectionId} already exists.");
            }

            _context.PlantFloorSection.Add(plantFloorSection);
            await _context.SaveChangesAsync();

            return plantFloorSection;
        }

        public async Task<List<PlantFloorSectionModel>> GetAllPlantFloorSectionsAsync()
        {
            return await _context.PlantFloorSection.ToListAsync();
        }

        public async Task<PlantFloorSectionModel> GetSectionById(int sectionId)
        {
            return await _context.PlantFloorSection
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);
        }
        
        public async Task<PlantFloorSectionModel> GetSectionByCodeAsync(string sectionCode)
        {
            return await _context.PlantFloorSection
                .FirstOrDefaultAsync(s => s.SectionCode == sectionCode);
        }


        public async Task<bool> DeletePlantFloorSectionByCodeAsync(int sectionId)
        {
            var section = await _context.PlantFloorSection
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
            {
                return false;
            }

            _context.PlantFloorSection.Remove(section);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PlantFloorSectionModel> UpdatePlantFloorSectionAsync(int sectionId, string name)
        {
            var section = await _context.PlantFloorSection
                .FirstOrDefaultAsync(s => s.SectionId == sectionId);

            if (section == null)
            {
                throw new Exception($"Section with code {sectionId} not found.");
            }

            section.name = name;
            section.SectionId = sectionId;

            _context.PlantFloorSection.Update(section);
            await _context.SaveChangesAsync();

            return section;
        }
    }
}
