using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services
{
    // Service responsável pela lógica usada no Portal do Cliente
    public class ClientPortalService : IClientPortalService
    {
        // Repository responsável por obter as encomendas e os produtos do cliente
        private readonly IClientPortalRepository _repo;

        // Service responsável por calcular o tempo estimado de conclusão dos produtos
        private readonly IEtaPredictionService _eta;

        // Reutiliza o repositório de admin (só leitura aqui) em vez de duplicar
        // acesso a dados de CarModel/Config — evita 2 caminhos para a mesma tabela.
        private readonly ICarModelRepository _carModelRepo;

        // O ASP.NET injeta automaticamente os repositories e services necessários
        public ClientPortalService(
            IClientPortalRepository repo,
            IEtaPredictionService eta,
            ICarModelRepository carModelRepo)
        {
            _repo = repo;
            _eta = eta;
            _carModelRepo = carModelRepo;
        }

        // Devolve todos os modelos de veículos disponíveis para o cliente
        public async Task<List<CarModelDTO>> GetModelsAsync()
        {
            // Obtém os modelos através do repository
            var models = await _carModelRepo.GetAll();

            // Converte cada modelo para DTO antes de o devolver
            return models
                .Select(m => new CarModelDTO(
                    m.Id,
                    m.Name,
                    m.Version,
                    m.Type
                ))
                .ToList();
        }

        // Devolve um modelo específico através do seu ID
        public async Task<CarModelDTO?> GetModelAsync(int modelId)
        {
            // Procura o modelo
            var model = await _carModelRepo.GetById(modelId);

            // Se não existir, devolve null.
            // Caso exista, converte-o para DTO.
            return model == null
                ? null
                : new CarModelDTO(
                    model.Id,
                    model.Name,
                    model.Version,
                    model.Type
                );
        }

        // Devolve as configurações e opções disponíveis para um modelo
        public async Task<List<ClientModelConfigDTO>?> GetModelConfigsAsync(int modelId)
        {
            // Verifica primeiro se o modelo existe
            if (!await _carModelRepo.Exists(modelId))
                return null;

            // Obtém as configurações do modelo, incluindo as opções
            var configs = await _carModelRepo.GetConfigsWithOptions(modelId);

            // Converte as configurações e opções para DTOs
            return configs
                .Select(c => new ClientModelConfigDTO
                {
                    Id = c.Id,
                    Item = c.Item,
                    AllowMultiple = c.AllowMultiple,

                    // Converte cada opção de configuração para ConfigOptionDTO
                    Options = c.ConfigOptions
                        .Select(o => new ConfigOptionDTO(
                            o.Id,
                            o.ConfigId,
                            o.Value,
                            o.IsDefault
                        ))
                        .ToList()
                })
                .ToList();
        }

        // Devolve o resumo das encomendas pertencentes ao cliente
        public async Task<List<ClientOrderSummaryDTO>> GetOrdersAsync(int appUserId)
        {
            // Obtém as encomendas do cliente
            var orders = await _repo.GetOrdersByClientAsync(appUserId);

            // Para cada encomenda com carros pendentes, prevê a conclusão de cada um
            // e usa a mais tardia como "previsão de conclusão da encomenda".
            var etaTasks = orders.Select(async o =>
            {
                // Se todos os produtos estiverem concluídos, não é necessário calcular ETA
                if (o.PendingProductIds.Count == 0)
                    return;

                // Calcula as previsões dos produtos pendentes em paralelo
                var predictions = await Task.WhenAll(
                    o.PendingProductIds.Select(async pid =>
                    {
                        try
                        {
                            return await _eta.PredictForProduct(pid);
                        }
                        catch
                        {
                            // Se a previsão de um produto falhar,
                            // devolve null sem impedir as restantes
                            return null;
                        }
                    })
                );

                // Procura a previsão mais tardia.
                // Essa previsão representa quando a encomenda completa deverá terminar.
                var latest = predictions
                    .Where(p => p != null)
                    .OrderByDescending(p => p!.EstimatedCompletion)
                    .FirstOrDefault();

                if (latest != null)
                {
                    // Guarda o ETA estimado da encomenda
                    o.EtaUtc = latest.EstimatedCompletion;

                    // Se existe uma data de treino, considera-se que a previsão
                    // foi baseada num modelo treinado
                    o.EtaIsMlPrediction = latest.ModelTrainedAt != null;
                }
            });

            // Aguarda o cálculo dos ETAs de todas as encomendas
            await Task.WhenAll(etaTasks);

            return orders;
        }

        // Devolve os detalhes de uma encomenda específica do cliente
        public async Task<ClientOrderDetailDTO?> GetOrderDetailAsync(
            int orderId,
            int appUserId)
        {
            // Procura a encomenda, garantindo que pertence ao cliente
            var detail = await _repo.GetOrderDetailAsync(orderId, appUserId);

            if (detail == null)
                return null;

            // Calcula ETA apenas para produtos não concluídos, em paralelo
            var pending = detail.Products
                .Where(p => !p.IsCompleted)
                .ToList();

            // Cria uma tarefa de previsão para cada produto pendente
            var etaTasks = pending.Select(async p =>
            {
                try
                {
                    // Calcula a previsão para o produto
                    var result = await _eta.PredictForProduct(p.ProductId);

                    if (result != null)
                    {
                        // Guarda a data estimada de conclusão
                        p.EtaUtc = result.EstimatedCompletion;

                        // ModelTrainedAt preenchido = previsão real do modelo ML;
                        // null = fallback/estimativa simples (ainda sem modelo treinado).
                        p.EtaIsMlPrediction = result.ModelTrainedAt != null;
                    }
                }
                catch
                {
                    // ETA não disponível — produto continua visível sem ETA
                }
            });

            // Aguarda todas as previsões
            await Task.WhenAll(etaTasks);

            return detail;
        }
    }
}