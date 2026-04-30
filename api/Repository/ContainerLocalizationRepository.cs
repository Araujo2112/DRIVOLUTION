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
    public class ContainerLocalizationRepository : IContainerLocalizationRepository
    {
        private readonly ApplicationDbContext _context;

        public ContainerLocalizationRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<ContainerLocalizationModel> CreateContainerLocalizationAsync(ContainerLocalizationModel localization)
        {
            _context.ContainerLocalization.Add(localization);
  
            await _context.SaveChangesAsync();
            return localization;  
        }

       
        public async Task<List<ContainerLocalizationModel>> GetAllContainerLocalizationsAsync()
        {
         
            return await _context.ContainerLocalization.ToListAsync();
        }


        public async Task<ContainerLocalizationModel> GetContainerLocalizationByIdAsync(int containerId)
        {
            return await _context.ContainerLocalization
                .FirstOrDefaultAsync(c => c.ContainerId == containerId)
                ?? throw new Exception($"Localization for container with ID {containerId} not found.");
        }

       
        public async Task<ContainerLocalizationModel> UpdateContainerLocalizationAsync(int id, int containerId, int sectionId, DateTime datetime)
        {
           
            var existingLocalization = await _context.ContainerLocalization
                .FirstOrDefaultAsync(c => c.Id == id);

            if (existingLocalization == null)
            {
                throw new Exception($"Localization with ID {id} not found.");
            }
            
            _context.ContainerLocalization.Remove(existingLocalization);
            await _context.SaveChangesAsync();  
     
            var newLocalization = new ContainerLocalizationModel
            {
                Id = id,  
                ContainerId = containerId,
                SectionId = sectionId,  
                Datetime = datetime 
            };

            
            await _context.ContainerLocalization.AddAsync(newLocalization);
            await _context.SaveChangesAsync();  

            return newLocalization;  
        }

      
        public async Task<bool> DeleteContainerLocalizationByIdAsync(int containerId)
        {
        
            var localization = await _context.ContainerLocalization
                .FirstOrDefaultAsync(c => c.ContainerId == containerId);

            if (localization == null)
            {
                return false;  
            }

           
            _context.ContainerLocalization.Remove(localization);
            await _context.SaveChangesAsync();  

            return true;  
        }
        
        public async Task<ContainerLocalizationModel> GetContainerLocalizationByContainerAndSectionAsync(int containerId, int sectionId)
        {
            return await _context.ContainerLocalization
                .FirstOrDefaultAsync(cl => cl.ContainerId == containerId && cl.SectionId == sectionId);
        }
        
        public async Task<ContainerLocalizationModel> GetLastContainerLocalizationAsync(int containerId)
        {
            return await _context.ContainerLocalization
                .Include(cl => cl.PlantFloorSection) // <-- necessário
                .Where(cl => cl.ContainerId == containerId)
                .OrderByDescending(cl => cl.Datetime)
                .FirstOrDefaultAsync();
        }
        
    }
}
