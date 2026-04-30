using ApiTexPact.Models;
using ApiTexPact.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiTexPact.Repository.Interface
{
    public interface ILotOfRawMaterialRepository
    {
        Task<LotOfRawMaterialModel> CreatelotAsync(LotOfRawMaterialModel lot);
        Task<List<LotOfRawMaterialModel>> GetAlllotsAsync();
        Task<LotOfRawMaterialModel> GetlotByCodeAsync(int lotId);
        Task<bool> DeletelotByCodeAsync(int lotId);
        Task<LotOfRawMaterialModel> UpdatelotAsync(
            int lotId,
            string lotNumber,
            string lotCode,
            int lotQuantity,
            string lotUnit);
    }
}