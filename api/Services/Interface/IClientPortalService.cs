using Drivolution.DTO;

namespace Drivolution.Services.Interface
{
    public interface IClientPortalService
    {
        Task<List<ClientOrderSummaryDTO>> GetOrdersAsync(int appUserId);
        Task<ClientOrderDetailDTO?> GetOrderDetailAsync(int orderId, int appUserId);
    }
}