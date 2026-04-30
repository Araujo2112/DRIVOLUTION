using ApiTexPact.Models;
using System.Threading.Tasks;

namespace ApiTexPact.Repository.Interface
{
    public interface ICheckpointRepository
    {
        Task<CheckpointModel> CreateCheckpointAsync(CheckpointModel checkpoint);
        Task<List<CheckpointModel>> GetAllCheckpointsFromDbAsync();
        Task<CheckpointModel> GetCheckpointByIdAsync(int checkpointId);
        Task<bool> DeleteCheckpointByIdAsync(int checkpointId);
        
        Task<CheckpointModel> UpdateCheckpointAsync(
            int checkpointId,
            string name,
            bool status,
            int sectionId);
    }
}