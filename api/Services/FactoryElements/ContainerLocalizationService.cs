using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Service.Interface;
using ApiTexPact.Services.Interface.Containers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Services.Containers
{
    public class ContainerLocalizationService : IContainerLocalizationService
    {
        private readonly IContainerLocalizationRepository _repository;

        public ContainerLocalizationService(IContainerLocalizationRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }
        
        public async Task<List<ContainerLocalizationModel>> GetAllContainerLocalizationsAsync()
        {
            return await _repository.GetAllContainerLocalizationsAsync();
        }
        
        public async Task<ContainerLocalizationModel> GetContainerLocalizationByIdAsync(int id)
        {
            var localization = await _repository.GetContainerLocalizationByIdAsync(id);
            if (localization == null)
            {
                throw new Exception($"Localization for container with ID {id} not found.");
            }
            return localization;
        }

     
        public async Task<ContainerLocalizationModel> CreateContainerLocalizationAsync(ContainerLocalizationModel localization)
        {

            return await _repository.CreateContainerLocalizationAsync(localization);
        }


        public async Task<ContainerLocalizationModel> UpdateContainerLocalizationAsync(int id, int containerId, int sectionId, DateTime datetime)
        {
            var existingLocalization = await _repository.GetContainerLocalizationByIdAsync(id);
            if (existingLocalization == null)
            {
                throw new Exception($"Localization with ID {id} not found.");
            }

            return await _repository.UpdateContainerLocalizationAsync(id, containerId, sectionId, datetime);
        }

        public async Task<bool> DeleteContainerLocalizationByIdAsync(int id)
        {
            var existingLocalization = await _repository.GetContainerLocalizationByIdAsync(id);
            if (existingLocalization == null)
            {
                throw new Exception($"Localization with ID {id} not found.");
            }

            return await _repository.DeleteContainerLocalizationByIdAsync(id);
        }
    }
}
