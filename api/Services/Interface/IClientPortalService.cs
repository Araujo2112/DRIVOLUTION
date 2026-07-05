using Drivolution.DTO;

namespace Drivolution.Services.Interface
{
    public interface IClientPortalService
    {
        Task<List<ClientOrderSummaryDTO>> GetOrdersAsync(int appUserId);
        Task<ClientOrderDetailDTO?> GetOrderDetailAsync(int orderId, int appUserId);
        Task<List<CarModelDTO>> GetModelsAsync();
        Task<CarModelDTO?> GetModelAsync(int modelId);
        Task<List<ClientModelConfigDTO>?> GetModelConfigsAsync(int modelId);
    }
}