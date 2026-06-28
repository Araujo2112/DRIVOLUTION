using Drivolution.DTO.Client;

namespace Drivolution.Repository.Interface;

public interface IClientPortalRepository
{
    Task<List<ClientOrderSummaryDTO>> GetOrders(int appUserId);

    Task<List<ClientOrderProductDTO>> GetProducts(int appUserId, int orderId);
}