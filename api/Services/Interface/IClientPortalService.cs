using Drivolution.DTO.Client;

namespace Drivolution.Services.Interface;

public interface IClientPortalService
{
    Task<List<ClientOrderSummaryDTO>> GetOrders(int appUserId);

    Task<List<ClientOrderProductDTO>> GetProducts(int appUserId, int orderId);
}