using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

[ApiController]
[Route("api/ml")]
[Authorize(Roles = "admin")]
public class MlController : ControllerBase
{
    private readonly IModelTrainingService _trainingService;

    public MlController(IModelTrainingService trainingService)
    {
        _trainingService = trainingService;
    }

    [HttpPost("retrain")]
    public async Task<IActionResult> Retrain()
    {
        var result = await _trainingService.RunTraining();
        if (!result.Success) return StatusCode(500, result);
        return Ok(result);
    }
}