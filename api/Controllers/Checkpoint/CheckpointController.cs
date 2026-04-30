using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApiTexPact.DTO;
using ApiTexPact.Services;

namespace ApiTexPact.Controllers.Checkpoint;

[Route("api/[controller]")]
[ApiController]
public class CheckpointController : ControllerBase
{
    private readonly ICheckpointService _service;

    public CheckpointController(ICheckpointService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCheckpointsAsync()
    {
        var checkpoints = await _service.GetAllCheckpointsAsync();
        return Ok(checkpoints);
    }

    [HttpGet("{checkpointId}")]
    public async Task<IActionResult> GetCheckpointByCode(int checkpointId)
    {
        try
        {
            var checkpoint = await _service.GetCheckpointByIdAsync(checkpointId);
            return Ok(checkpoint);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCheckpoint([FromBody] CheckpointDTO checkpointDto)
    {
        try
        {
        
            var checkpoint = new CheckpointModel
            {
                CheckpointCode = checkpointDto.CheckpointCode,
                Name = checkpointDto.Name,
                Status = checkpointDto.Status,
                SectionId = checkpointDto.SectionId
            };
            
            var newCheckpoint = await _service.CreateCheckpointAsync(checkpoint);
            
            return CreatedAtAction(
                nameof(GetCheckpointByCode),
                new { checkpointId = newCheckpoint.CheckpointId },
                newCheckpoint
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCheckpoint([FromBody] CheckpointDTO checkpointDto)
    {
        try
        {
            var result = await _service.UpdateCheckpointAsync(
                checkpointDto.CheckpointId,
                checkpointDto.Name,
                checkpointDto.Status,
                checkpointDto.SectionId 
            );

            if (result == null)
            {
                return NotFound($"Checkpoint with id {checkpointDto.CheckpointId} not found.");
            }

            return Ok($"Checkpoint with id {checkpointDto.CheckpointId} updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{checkpointId}")]
    public async Task<IActionResult> DeleteCheckpoint(int checkpointId)
    {
        try
        {
            var result = await _service.DeleteCheckpointAsync(checkpointId);
            if (!result)
            {
                return NotFound($"Checkpoint with code {checkpointId} not found.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}