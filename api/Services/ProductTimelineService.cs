using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Implementa o contrato definido em IProductTimelineService
public class ProductTimelineService : IProductTimelineService
{
    // Repository responsável por ir buscar os dados da timeline
    private readonly IProductTimelineRepository _repository;

    // Service responsável por calcular previsões de duração/ETA
    private readonly IEtaPredictionService _etaService;

    // O ASP.NET injeta automaticamente o repository e o service
    public ProductTimelineService(
        IProductTimelineRepository repository,
        IEtaPredictionService etaService)
    {
        _repository = repository;
        _etaService = etaService;
    }

    // Verifica se existe um produto com este ID
    public Task<bool> ProductExists(int productId) =>
        _repository.ProductExists(productId);

    // Verifica se existe um produto com este número de série
    public Task<bool> ProductExistsBySerial(string serialNumber) =>
        _repository.ProductExistsBySerial(serialNumber);

    // Obtém a timeline de um produto através do ID
    public async Task<ProductTimelineResultDTO?> GetTimeline(int productId)
    {
        // Pede ao repository os dados das fases do produto
        var timeline = await _repository.GetTimeline(productId);

        // Organiza os dados e acrescenta a previsão, se possível
        return await BuildResult(timeline);
    }

    // Obtém a timeline através do número de série
    public async Task<ProductTimelineResultDTO?> GetTimelineBySerial(
        string serialNumber)
    {
        // Pede ao repository os dados das fases do produto
        var timeline =
            await _repository.GetTimelineBySerial(serialNumber);

        // Organiza os dados e acrescenta a previsão, se possível
        return await BuildResult(timeline);
    }

    // Constrói o resultado final da timeline
    private async Task<ProductTimelineResultDTO?> BuildResult(
        List<ProductTimelineDTO> timeline)
    {
        // Se não existirem fases, o produto ainda não tem timeline
        if (!timeline.Any())
            return null;

        // Procura a primeira fase que ainda não terminou
        // Uma fase está aberta quando EndedAt é null
        var openPhase =
            timeline.FirstOrDefault(t => t.EndedAt == null);

        // Se existir uma fase aberta, tenta calcular a sua conclusão estimada
        if (openPhase != null)
        {
            try
            {
                // Pede ao serviço de ETA uma previsão da duração
                // da fase atual, em segundos
                var prediction =
                    await _etaService
                        .PredictCurrentPhaseDurationSeconds(
                            openPhase.ProductId
                        );

                // Se foi possível obter uma previsão
                if (prediction != null)
                {
                    // Soma os segundos previstos à data de início da fase
                    var estimatedFinish =
                        openPhase.StartedAt
                            .AddSeconds(prediction.Seconds);

                    // Evita devolver uma previsão no passado
                    if (estimatedFinish < DateTime.UtcNow)
                        estimatedFinish = DateTime.UtcNow;

                    // Guarda a data estimada de conclusão na fase aberta
                    openPhase.EstimatedFinish = estimatedFinish;
                }
                else
                {
                    // Se não houver previsão disponível,
                    // a data estimada fica sem valor
                    openPhase.EstimatedFinish = null;
                }
            }
            catch
            {
                // Se ocorrer algum erro na previsão,
                // a timeline continua a ser devolvida sem ETA
                openPhase.EstimatedFinish = null;
            }
        }

        // Cria o objeto final que será enviado ao controller
        return new ProductTimelineResultDTO
        {
            // Como todas as linhas pertencem ao mesmo produto,
            // usa os dados do primeiro elemento
            ProductId = timeline.First().ProductId,
            ModelId = timeline.First().ModelId,
            SerialNumber = timeline.First().SerialNumber,

            // Se existir alguma fase aberta, o produto está em produção
            // Caso contrário, é considerado concluído
            Status = timeline.Any(t => t.EndedAt == null)
                ? "in_progress"
                : "completed",

            // Guarda a lista completa de fases
            Phases = timeline
        };
    }
}