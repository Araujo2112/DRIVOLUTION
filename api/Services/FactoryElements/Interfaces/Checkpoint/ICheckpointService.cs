using ApiTexPact.Models;
using System.Threading.Tasks;

namespace ApiTexPact.Services
{
    public interface ICheckpointService
    {
        Task<List<CheckpointModel>> GetAllCheckpointsAsync();
        Task<CheckpointModel> GetCheckpointByIdAsync(int checkpointId);
        Task<CheckpointModel> CreateCheckpointAsync(CheckpointModel checkpoint);
        
        Task<CheckpointModel> UpdateCheckpointAsync(
            int checkpointId,
            string name,
            bool status,
            int sectionId);

        Task<bool> DeleteCheckpointAsync(int checkpointId);
    }
}