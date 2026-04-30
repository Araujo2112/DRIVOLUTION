using ApiTextPact.Services;
using Microsoft.AspNetCore.Mvc;
using Models;
using Services;

namespace Controllers
{
    [ApiController]
    [Route("api/sensors")]
    public class MotionSensorController : ControllerBase
    {
        private readonly ISensorService _sensorService;

        public SensorController(ISensorService sensorService)
        {
            _sensorService = sensorService;
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateSensor(Guid id, [FromBody] SensorUpdateRequest request)
        {
            try
            {
                await _sensorService.UpdateSensorState(id, request.State);
                return Ok(new { message = "Sensor state updated successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class SensorUpdateRequest
    {
        public int State { get; set; } // 0 ou 1
    }
}