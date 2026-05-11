using ApiTexPact.Models;
namespace ApiTexPact.Repository.Interface.ClientOrder;
public interface IClientOrderRepository
{
    Task<IEnumerable<ClientOrderModel>> GetAll();
    Task<ClientOrderModel?> GetById(int id);
    Task<ClientOrderModel> Create(ClientOrderModel entity);
    Task Update(ClientOrderModel entity);
    Task Delete(int id);
    Task<bool> Exists(int id);
}
