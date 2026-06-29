using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services
{
    public class ClientPortalService : IClientPortalService
    {
        private readonly IClientPortalRepository _repo;
        private readonly IEtaPredictionService _eta;

        public ClientPortalService(IClientPortalRepository repo, IEtaPredictionService eta)
        {
            _repo = repo;
            _eta = eta;
        }

        public Task<List<ClientOrderSummaryDTO>> GetOrdersAsync(int appUserId)
            => _repo.GetOrdersByClientAsync(appUserId);

        public async Task<ClientOrderDetailDTO?> GetOrderDetailAsync(int orderId, int appUserId)
        {
            var detail = await _repo.GetOrderDetailAsync(orderId, appUserId);
            if (detail == null) return null;

            // Calcula ETA apenas para produtos não concluídos, em paralelo
            var pending = detail.Products.Where(p => !p.IsCompleted).ToList();

            var etaTasks = pending.Select(async p =>
            {
                try
                {
                    var result = await _eta.PredictForProduct(p.ProductId);
                    if (result != null)
                    {
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

            await Task.WhenAll(etaTasks);

            return detail;
        }
    }
}