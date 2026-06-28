using Drivolution.DTO.Client;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ClientPortalService : IClientPortalService
{
    private readonly IClientPortalRepository _repository;

    public ClientPortalService(IClientPortalRepository repository)
    {
        _repository = repository;
    }

    public Task<List<ClientOrderSummaryDTO>> GetOrders(int appUserId)
    {
        return _repository.GetOrders(appUserId);
    }

    public Task<List<ClientOrderProductDTO>> GetProducts(int appUserId, int orderId)
    {
        return _repository.GetProducts(appUserId, orderId);
    }
}