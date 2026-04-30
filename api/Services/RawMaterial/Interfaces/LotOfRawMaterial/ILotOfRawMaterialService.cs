using ApiTexPact.Models;
using ApiTexPact.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Services
{
    public interface ILotOfRawMaterialService
    {
        Task<LotOfRawMaterialModel> CreateLotAsync(LotOfRawMaterialModel lot);
        Task<List<LotOfRawMaterialModel>> GetAllLotsAsync();
        Task<LotOfRawMaterialResponseDTO> GetLotByCodeAsync(int lotId);
        Task<bool> DeleteLotByCodeAsync(int lotId);
        Task<LotOfRawMaterialModel> UpdateLotAsync(
            int lotId,
            string lotNumber,
            string lotCode,
            int lotQuantity,
            string lotUnit);
    }
}