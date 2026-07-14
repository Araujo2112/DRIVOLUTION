using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável pelas previsões de duração e ETA dos produtos
public class EtaPredictionService : IEtaPredictionService
{
    // Repository usado para obter os dados do produto
    private readonly IProductRepository _productRepo;

    // Repository usado para obter as opções/configurações do produto
    private readonly IProductConfigRepository _productConfigRepo;

    // Repository usado para consultar as fases atuais dos produtos
    private readonly IProductPhaseRepository _productPhaseRepo;

    // Repository usado para obter a sequência de fases de cada modelo
    private readonly IPhaseSequenceRepository _phaseSequenceRepo;

    // Repository onde estão guardados os coeficientes usados nas previsões
    private readonly IPhaseTimeCoefficientRepository _coefficientRepo;

    // Calculador local usado se o serviço de Machine Learning não responder
    private readonly IPhaseTimeWeightCalculator _weightCalculator;

    // Cliente HTTP usado para comunicar com o serviço externo de Machine Learning
    private readonly HttpClient _httpClient;

    // Endereço do serviço de Machine Learning
    private readonly string _mlServiceUrl;

    // O ASP.NET injeta automaticamente os repositories,
    // o calculador e a configuração da aplicação
    public EtaPredictionService(
        IProductRepository productRepo,
        IProductConfigRepository productConfigRepo,
        IProductPhaseRepository productPhaseRepo,
        IPhaseSequenceRepository phaseSequenceRepo,
        IPhaseTimeCoefficientRepository coefficientRepo,
        IPhaseTimeWeightCalculator weightCalculator,
        IConfiguration configuration)
    {
        _productRepo = productRepo;
        _productConfigRepo = productConfigRepo;
        _productPhaseRepo = productPhaseRepo;
        _phaseSequenceRepo = phaseSequenceRepo;
        _coefficientRepo = coefficientRepo;
        _weightCalculator = weightCalculator;

        // Obtém o endereço do serviço ML das variáveis de configuração.
        // Se não existir, usa este endereço por defeito.
        _mlServiceUrl =
            configuration["ML_SERVICE_URL"]
            ?? "http://ml-service:8000";

        // Cria o cliente usado para fazer pedidos ao serviço de ML
        _httpClient = new HttpClient
        {
            // Se o serviço demorar mais de 3 segundos, o pedido é cancelado
            Timeout = TimeSpan.FromSeconds(3)
        };
    }

    // Calcula a data e hora estimadas para o fim da fase atual
    public async Task<DateTime?> PredictCurrentPhaseFinish(int productId)
    {
        // Obtém a duração total prevista da fase atual, em segundos
        var result =
            await PredictCurrentPhaseDurationSecondsInternal(productId);

        // Se não for possível calcular uma previsão, devolve null
        if (result == null) return null;

        // Procura a fase atual do produto
        var currentPhase =
            await _productPhaseRepo.GetCurrentByProduct(productId);

        if (currentPhase == null) return null;

        // Soma a duração prevista ao momento em que a fase começou
        var predictedFinish = currentPhase.DatetimeIni.AddSeconds(
            (double)result.Value.Seconds
        );

        // Evita devolver uma data de conclusão que já esteja no passado
        if (predictedFinish < DateTime.UtcNow)
            return DateTime.UtcNow;

        return predictedFinish;
    }

    // Devolve a duração prevista da fase atual em segundos
    public async Task<PhaseDurationPredictionDTO?>
        PredictCurrentPhaseDurationSeconds(int productId)
    {
        // Chama o método interno que faz o cálculo verdadeiro
        var result =
            await PredictCurrentPhaseDurationSecondsInternal(productId);

        if (result == null) return null;

        // Converte o resultado interno para um DTO
        return new PhaseDurationPredictionDTO
        {
            Seconds = (int)result.Value.Seconds,

            // Indica se o resultado veio de um modelo treinado
            IsMlPrediction = result.Value.IsMl
        };
    }

    // Método interno que reúne os dados necessários
    // e calcula a duração da fase atual
    private async Task<(decimal Seconds, bool IsMl)?>
        PredictCurrentPhaseDurationSecondsInternal(int productId)
    {
        // Procura o produto
        var product = await _productRepo.GetById(productId);

        if (product == null) return null;

        // Procura a fase atual do produto
        var currentPhase =
            await _productPhaseRepo.GetCurrentByProduct(productId);

        if (currentPhase == null) return null;

        // Obtém os IDs das opções escolhidas para este produto
        var selectedOptionIds =
            (await _productConfigRepo.GetByProduct(productId))
            .Select(pc => pc.ConfigOptionId)
            .ToHashSet();

        // Obtém os coeficientes resultantes do treino
        var coefficients =
            (await _coefficientRepo.GetAll()).ToList();

        // Obtém a linha de produção através da workstation atual
        var lineId =
            currentPhase.Workstation.ProductionLineId;

        // Usa a duração estimada da fase.
        // Se não existir, usa 1800 segundos, ou seja, 30 minutos.
        var fallbackSeconds =
            currentPhase.ManufacturingPhase.EstimatedDuration ?? 1800;

        // Calcula a duração prevista da fase
        return await PredictPhaseDurationSeconds(
            currentPhase.ManufacturingPhaseId,
            product.ModelId,
            selectedOptionIds,
            lineId,
            coefficients,
            fallbackSeconds
        );
    }

    // Calcula a ETA total de um produto,
    // somando o tempo restante de todas as fases
    public async Task<EtaResultDTO?> PredictForProduct(int productId)
    {
        // Procura o produto
        var product = await _productRepo.GetById(productId);

        if (product == null) return null;

        // Obtém as opções selecionadas para o produto
        var selectedOptionIds =
            (await _productConfigRepo.GetByProduct(productId))
            .Select(pc => pc.ConfigOptionId)
            .ToHashSet();

        // Obtém a fase atual do produto, caso exista
        var currentPhase =
            await _productPhaseRepo.GetCurrentByProduct(productId);

        // Obtém a sequência de fases definida para o modelo do produto
        var sequence =
            (await _phaseSequenceRepo.GetByModel(product.ModelId))
            .ToList();

        // Obtém os coeficientes utilizados nas previsões
        var coefficients =
            (await _coefficientRepo.GetAll()).ToList();

        // Obtém a data do último treino
        var trainedAt =
            await _coefficientRepo.GetLastTrainedAt();

        var now = DateTime.UtcNow;

        // Se não existe fase atual, mas já existe uma data de produção,
        // o produto é considerado concluído
        if (currentPhase == null &&
            product.ProductionDate != null)
        {
            return new EtaResultDTO(
                product.Id,
                product.SerialNumber ?? "",
                product.ProductionDate.Value,
                "Concluído",
                0,
                trainedAt
            );
        }

        // Lista onde serão guardadas as fases que ainda faltam
        List<PhaseSequenceModel> remainingPhases;

        // Tempo que já passou na fase atual
        decimal elapsedInCurrentSeconds = 0;

        // Linha de produção atual
        int? lineId = null;

        // Nome da fase atual
        string currentPhaseName;

        // Se o produto ainda não começou
        if (currentPhase == null)
        {
            // Todas as fases da sequência ainda estão em falta
            remainingPhases = sequence;
            currentPhaseName = "Não iniciado";
        }
        else
        {
            // Procura a posição da fase atual na sequência
            var currentIndex = sequence.FindIndex(ps =>
                ps.ManufacturingPhaseId ==
                currentPhase.ManufacturingPhaseId
            );

            // Se a fase atual não pertencer à sequência do modelo,
            // não é possível calcular corretamente
            if (currentIndex == -1) return null;

            // Mantém apenas a fase atual e as fases seguintes
            remainingPhases =
                sequence.Skip(currentIndex).ToList();

            // Calcula há quantos segundos a fase atual começou
            elapsedInCurrentSeconds =
                (decimal)(now - currentPhase.DatetimeIni)
                .TotalSeconds;

            // Evita valores negativos
            if (elapsedInCurrentSeconds < 0)
                elapsedInCurrentSeconds = 0;

            lineId =
                currentPhase.Workstation.ProductionLineId;

            currentPhaseName =
                currentPhase.ManufacturingPhase.Name.Trim();
        }

        // Total de segundos que ainda faltam para concluir o produto
        decimal totalRemainingSeconds = 0;

        // Percorre todas as fases que ainda faltam
        for (int i = 0; i < remainingPhases.Count; i++)
        {
            var phaseSeq = remainingPhases[i];

            // Se a fase não tiver duração estimada,
            // usa 30 minutos como valor alternativo
            var fallbackSeconds =
                phaseSeq.ManufacturingPhase.EstimatedDuration
                ?? 1800;

            // Calcula a duração prevista desta fase
            var prediction = await PredictPhaseDurationSeconds(
                phaseSeq.ManufacturingPhaseId,
                product.ModelId,
                selectedOptionIds,
                lineId,
                coefficients,
                fallbackSeconds
            );

            // Na fase atual, retira o tempo que já passou.
            // Nas fases futuras, usa a duração total prevista.
            decimal remainingForThisPhase =
                i == 0 && currentPhase != null
                    ? prediction.Seconds -
                      elapsedInCurrentSeconds
                    : prediction.Seconds;

            // Evita adicionar tempos negativos
            if (remainingForThisPhase < 0)
                remainingForThisPhase = 0;

            // Soma o tempo restante desta fase ao total
            totalRemainingSeconds += remainingForThisPhase;
        }

        // Soma o tempo total restante à data atual
        var eta = now.AddSeconds(
            (double)totalRemainingSeconds
        );

        // Devolve o resultado final
        return new EtaResultDTO(
            product.Id,
            product.SerialNumber ?? "",
            eta,
            currentPhaseName,
            (int)totalRemainingSeconds,
            trainedAt
        );
    }

    // Calcula as ETAs dos produtos que estão numa linha de produção
    public async Task<List<EtaResultDTO>>
        PredictForProductionLine(int productionLineId)
    {
        // Obtém todas as fases abertas dessa linha
        var openPhases =
            await _productPhaseRepo
                .GetAllOpenByProductionLine(productionLineId);

        var results = new List<EtaResultDTO>();

        // Calcula individualmente a ETA de cada produto
        foreach (var phase in openPhases)
        {
            var result =
                await PredictForProduct(phase.ProductId);

            if (result != null)
                results.Add(result);
        }

        return results;
    }

    // Calcula a duração prevista da fase atual
    // para vários produtos WIP de uma só vez
    public async Task<Dictionary<int, PhaseDurationPredictionDTO>>
        PredictCurrentPhaseDurationsForWip(
            IReadOnlyList<WipItemDTO> items)
    {
        // Se não existirem produtos, devolve um dicionário vazio
        if (items.Count == 0)
        {
            return new Dictionary<
                int,
                PhaseDurationPredictionDTO
            >();
        }

        // Obtém os IDs de todos os produtos
        var productIds =
            items.Select(i => i.ProductId).ToList();

        // Obtém os coeficientes apenas uma vez
        var coefficients =
            (await _coefficientRepo.GetAll()).ToList();

        // Obtém as configurações de todos os produtos de uma só vez
        var allConfigs =
            await _productConfigRepo.GetByProducts(productIds);

        // Agrupa as opções por produto
        var configsByProduct = allConfigs
            .GroupBy(pc => pc.ProductId)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(pc => pc.ConfigOptionId)
                    .ToHashSet()
            );

        // Obtém as fases atuais de todos os produtos
        var openPhases =
            await _productPhaseRepo
                .GetCurrentByProducts(productIds);

        // Organiza as fases por ID do produto
        var phasesByProduct =
            openPhases.ToDictionary(pp => pp.ProductId);

        // O resultado relaciona cada ProductId com a sua previsão
        var result =
            new Dictionary<int, PhaseDurationPredictionDTO>();

        foreach (var item in items)
        {
            // Tenta encontrar a fase atual deste produto
            if (!phasesByProduct.TryGetValue(
                    item.ProductId,
                    out var currentPhase))
            {
                continue;
            }

            // Obtém as opções do produto.
            // Se não tiver nenhuma, utiliza um conjunto vazio.
            var selectedOptionIds =
                configsByProduct.TryGetValue(
                    item.ProductId,
                    out var opts
                )
                    ? opts
                    : new HashSet<int>();

            var lineId =
                currentPhase.Workstation.ProductionLineId;

            var fallbackSeconds =
                currentPhase.ManufacturingPhase
                    .EstimatedDuration
                ?? 1800;

            // Calcula a duração prevista
            var prediction = await PredictPhaseDurationSeconds(
                currentPhase.ManufacturingPhaseId,
                item.ModelId,
                selectedOptionIds,
                lineId,
                coefficients,
                fallbackSeconds
            );

            // Guarda a previsão usando o ID do produto como chave
            result[item.ProductId] =
                new PhaseDurationPredictionDTO
                {
                    Seconds = (int)prediction.Seconds,
                    IsMlPrediction = prediction.IsMl
                };
        }

        return result;
    }

    // Método central que decide que tipo de previsão utilizar
    private async Task<(decimal Seconds, bool IsMl)>
        PredictPhaseDurationSeconds(
            int phaseId,
            int modelId,
            HashSet<int> optionIds,
            int? lineId,
            List<PhaseTimeCoefficientModel> coefficients,
            int fallbackSeconds)
    {
        // Primeiro tenta obter uma previsão do serviço externo de ML
        var mlPrediction = await TryPredictWithMlService(
            phaseId,
            modelId,
            optionIds,
            lineId
        );

        // Se o serviço ML respondeu com um valor válido,
        // devolve esse valor
        if (mlPrediction.HasValue &&
            mlPrediction.Value > 0)
        {
            return (mlPrediction.Value, true);
        }

        // Se o serviço ML falhar, utiliza o calculador local
        var seconds =
            _weightCalculator.PredictPhaseDurationSeconds(
                phaseId,
                modelId,
                optionIds,
                lineId,
                coefficients,
                fallbackSeconds
            );

        // Verifica se o cálculo local utilizou coeficientes treinados
        var isMl =
            _weightCalculator.HasTrainedIntercept(
                phaseId,
                coefficients
            );

        return (seconds, isMl);
    }

    // Tenta comunicar com o serviço externo de Machine Learning
    private async Task<decimal?> TryPredictWithMlService(
        int phaseId,
        int modelId,
        HashSet<int> optionIds,
        int? lineId)
    {
        try
        {
            // Cria o objeto que será enviado em JSON
            var payload = new MlPredictionRequest(
                phaseId,
                modelId,
                optionIds.ToList(),
                lineId,

                // Valor alternativo enviado ao serviço de ML
                1800
            );

            // Faz um pedido POST para o endpoint /predict
            var response =
                await _httpClient.PostAsJsonAsync(
                    $"{_mlServiceUrl}/predict",
                    payload
                );

            // Se a resposta HTTP indicar erro, devolve null
            if (!response.IsSuccessStatusCode)
                return null;

            // Converte a resposta JSON para MlPredictionResponse
            var result =
                await response.Content
                    .ReadFromJsonAsync<MlPredictionResponse>();

            // Valida o valor devolvido
            if (result == null ||
                result.PredictedSeconds <= 0)
            {
                return null;
            }

            return result.PredictedSeconds;
        }
        catch
        {
            // Se o serviço estiver desligado, exceder o tempo
            // ou ocorrer outro erro, devolve null.
            // Assim, o sistema pode usar o cálculo local.
            return null;
        }
    }

    // Representa o JSON enviado para o serviço de Machine Learning
    private sealed record MlPredictionRequest(
        [property: JsonPropertyName("phase_id")]
        int PhaseId,

        [property: JsonPropertyName("model_id")]
        int ModelId,

        [property: JsonPropertyName("option_ids")]
        List<int> OptionIds,

        [property: JsonPropertyName("line_id")]
        int? LineId,

        [property: JsonPropertyName("fallback_seconds")]
        int FallbackSeconds
    );

    // Representa o JSON recebido do serviço de Machine Learning
    private sealed record MlPredictionResponse(
        [property: JsonPropertyName("predicted_seconds")]
        decimal PredictedSeconds,

        [property: JsonPropertyName("is_ml_prediction")]
        bool IsMlPrediction
    );
}