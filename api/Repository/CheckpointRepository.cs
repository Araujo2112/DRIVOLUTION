using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Collections.Generic;
using ApiTexPact.Data;

namespace ApiTexPact.Repository
{
    public class CheckpointRepository : ICheckpointRepository
    {
        private readonly ApplicationDbContext _context;

        public CheckpointRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<CheckpointModel> CreateCheckpointAsync(CheckpointModel checkpoint)
        {
            var existingCheckpoint = await _context.Checkpoints
                .FirstOrDefaultAsync(c => c.CheckpointId == checkpoint.CheckpointId);

            if (existingCheckpoint != null)
            {
                throw new Exception($"Checkpoint with id {checkpoint.CheckpointId} already exists.");
            }
            
            _context.Checkpoints.Add(checkpoint);
            await _context.SaveChangesAsync();

            return checkpoint;
        }

        public async Task<List<CheckpointModel>> GetAllCheckpointsFromDbAsync()
        {
            return await _context.Checkpoints
                .ToListAsync();
        }

        public async Task<CheckpointModel> GetCheckpointByIdAsync(int checkpointId)
        {
            return await _context.Checkpoints
                .FirstOrDefaultAsync(c => c.CheckpointId == checkpointId)
                ?? throw new Exception($"Checkpoint with id {checkpointId} not found.");
        }

        public async Task<CheckpointModel> UpdateCheckpointAsync(
            int checkpointId,
            string name,
            bool status,
            int sectionId)
        {
            var checkpoint = await _context.Checkpoints
                .FirstOrDefaultAsync(c => c.CheckpointId == checkpointId);

            if (checkpoint == null)
            {
                throw new Exception($"Checkpoint with code {checkpointId} not found.");
            }

            checkpoint.Name = name;
            checkpoint.Status = status;
            checkpoint.SectionId = sectionId;

            _context.Checkpoints.Update(checkpoint);
            await _context.SaveChangesAsync();

            return checkpoint;
        }

        public async Task<bool> DeleteCheckpointByIdAsync(int checkpointId)
        {
            var checkpoint = await _context.Checkpoints
                .FirstOrDefaultAsync(c => c.CheckpointId == checkpointId);

            if (checkpoint == null)
            {
                return false;
            }

            _context.Checkpoints.Remove(checkpoint);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
