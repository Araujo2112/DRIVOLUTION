using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiTexPact.Service.Interface;

namespace ApiTexPact.Service
{
    public class RawMaterialService : IRawMaterialService
    {
        private readonly IRawMaterialRepository _repository;

        public RawMaterialService(IRawMaterialRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RawMaterialModel> CreateRawMaterialAsync(RawMaterialModel rawMaterial)
        {
            return await _repository.CreateRawMaterialAsync(rawMaterial);
        }

        public async Task<List<RawMaterialModel>> GetAllRawMaterialsAsync()
        {
            return await _repository.GetAllRawMaterialsAsync();
        }

        public async Task<RawMaterialModel> GetRawMaterialByIdAsync(int id)
        {
            return await _repository.GetRawMaterialByIdAsync(id);
        }

        public async Task<RawMaterialModel> UpdateRawMaterialAsync(int id, RawMaterialModel rawMaterial)
        {
            var existingRawMaterial = await _repository.GetRawMaterialByIdAsync(id);
            if (existingRawMaterial == null)
            {
                throw new Exception($"RawMaterial with ID {id} not found.");
            }

            return await _repository.UpdateRawMaterialAsync(id, rawMaterial); 
        }

        public async Task<bool> DeleteRawMaterialByIdAsync(int id)
        {
            var existingRawMaterial = await _repository.GetRawMaterialByIdAsync(id);
            if (existingRawMaterial == null)
            {
                throw new Exception($"RawMaterial with ID {id} not found.");
            }

            return await _repository.DeleteRawMaterialByIdAsync(id);
        }
    }
}