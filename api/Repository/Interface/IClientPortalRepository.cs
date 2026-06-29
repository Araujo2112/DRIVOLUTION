using Drivolution.DTO;

namespace Drivolution.Repository.Interface
{
    public interface IClientPortalRepository
    {
        /// <summary>
        /// Devolve todas as encomendas associadas ao utilizador cliente (via app_user_id),
        /// com contagem de carros concluídos vs total.
        /// Um carro está concluído quando tem uma product_phase da fase "Inspeção" com datetime_end preenchido.
        /// </summary>
        Task<List<ClientOrderSummaryDTO>> GetOrdersByClientAsync(int appUserId);

        /// <summary>
        /// Devolve os produtos de uma encomenda específica do cliente, com fase atual.
        /// Não inclui ETA — calculado no Service para controlo de performance.
        /// </summary>
        Task<ClientOrderDetailDTO?> GetOrderDetailAsync(int orderId, int appUserId);
    }
}