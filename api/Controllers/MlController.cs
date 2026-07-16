using Drivolution.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Drivolution.Controllers;

// Indica que esta classe é um controller da API
[ApiController]

// Define a rota base: /api/ml
[Route("api/ml")]

// Apenas administradores podem iniciar um novo treino do modelo de Machine Learning
[Authorize(Roles = "admin")]
public class MlController : ControllerBase
{
    // Service responsável por executar o treino do modelo de Machine Learning
    private readonly IModelTrainingService _trainingService;

    // O ASP.NET injeta automaticamente o service necessário
    public MlController(IModelTrainingService trainingService)
    {
        _trainingService = trainingService;
    }

    // POST /api/ml/retrain
    // Executa um novo treino do modelo de Machine Learning
    [HttpPost("retrain")]
    public async Task<IActionResult> Retrain()
    {
        // Pede ao service para iniciar o processo de treino
        var result = await _trainingService.RunTraining();

        // Se ocorrer algum erro durante o treino devolve 500
        if (!result.Success)
            return StatusCode(500, result);

        // Caso o treino termine com sucesso devolve o resultado
        return Ok(result);
    }
}