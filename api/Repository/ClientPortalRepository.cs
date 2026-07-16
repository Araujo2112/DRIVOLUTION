using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository
{
    // Repository responsável por fornecer os dados apresentados
    // no portal do cliente
    public class ClientPortalRepository : IClientPortalRepository
    {
        // Contexto da base de dados
        private readonly ApplicationDbContext _context;

        // O ASP.NET injeta automaticamente o DbContext
        public ClientPortalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Obtém todas as encomendas pertencentes a um cliente
        public async Task<List<ClientOrderSummaryDTO>> GetOrdersByClientAsync(int appUserId)
        {
            // Encomendas do cliente com contagem de carros concluídos
            // Critério de "concluído": produto tem product_phase da fase "Inspeção" com datetime_end IS NOT NULL.
            // Trim() porque manufacturing_phase.name é VARCHAR sem normalização garantida na BD.
            var raw = await _context.ClientOrders
                // Filtra apenas as encomendas do cliente autenticado
                .Where(co => co.AppUserId == appUserId)

                // Seleciona apenas os dados necessários
                .Select(co => new
                {
                    co.Id,
                    co.OrderNumber,
                    co.OrderDate,

                    // Obtém todos os produtos pertencentes às ordens de fabrico
                    Products = co.ManufacturingOrders
                        .SelectMany(mo => mo.Products)
                        .Select(p => new
                        {
                            p.Id,

                            // Nome do modelo do veículo
                            ModelName = p.CarModel.Name,

                            // Verifica se o produto já terminou a fase de inspeção
                            IsCompleted = p.ProductPhases
                                .Any(pp => pp.ManufacturingPhase.Name.Contains("Inspeção")
                                           && pp.DatetimeEnd != null)
                        })
                        .ToList()
                })

                // Ordena as encomendas da mais recente para a mais antiga
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            // Converte os resultados para o DTO utilizado pelo portal
            var orders = raw.Select(o => new ClientOrderSummaryDTO
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,

                // Número total de veículos da encomenda
                TotalCars = o.Products.Count,

                // Número de veículos concluídos
                CompletedCars = o.Products.Count(p => p.IsCompleted),

                // Uma encomenda pode ter produtos de modelos diferentes (agrega vários
                // ManufacturingOrders); mostramos os modelos distintos em vez de assumir 1:1.
                ModelName = string.Join(" / ", o.Products.Select(p => p.ModelName).Distinct()),

                // Guarda os IDs dos produtos ainda não concluídos
                PendingProductIds = o.Products
                    .Where(p => !p.IsCompleted)
                    .Select(p => p.Id)
                    .ToList()
            }).ToList();

            // Preenche o campo Status com texto legível
            foreach (var o in orders)
                o.Status = $"{o.CompletedCars} de {o.TotalCars} concluídos";

            return orders;
        }

        // Obtém os detalhes completos de uma encomenda específica
        public async Task<ClientOrderDetailDTO?> GetOrderDetailAsync(int orderId, int appUserId)
        {
            // Procura a encomenda apenas se pertencer ao cliente autenticado
            var order = await _context.ClientOrders
                .Where(co => co.Id == orderId && co.AppUserId == appUserId)

                // Seleciona apenas a informação necessária
                .Select(co => new
                {
                    co.Id,
                    co.OrderNumber,
                    co.OrderDate,

                    Products = co.ManufacturingOrders
                        .SelectMany(mo => mo.Products)
                        .Select(p => new
                        {
                            p.Id,
                            p.SerialNumber,

                            // Modelo do veículo
                            ModelName = p.CarModel.Name,

                            // Verifica se o produto já terminou a produção
                            IsCompleted = p.ProductPhases
                                .Any(pp => pp.ManufacturingPhase.Name.Contains("Inspeção")
                                           && pp.DatetimeEnd != null),

                            // Obtém a fase atualmente em execução.
                            // Se não existir nenhuma fase ativa, considera o produto concluído.
                            CurrentPhase = p.ProductPhases
                                .Where(pp => pp.DatetimeEnd == null)
                                .OrderByDescending(pp => pp.DatetimeIni)
                                .Select(pp => pp.ManufacturingPhase.Name)
                                .FirstOrDefault() ?? "Concluído"
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            // Se a encomenda não existir ou não pertencer ao cliente
            if (order == null)
                return null;

            // Constrói o DTO final
            return new ClientOrderDetailDTO
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,

                // Converte todos os produtos para DTO
                Products = order.Products.Select(p => new ClientProductDTO
                {
                    ProductId = p.Id,
                    SerialNumber = p.SerialNumber,
                    ModelName = p.ModelName,
                    IsCompleted = p.IsCompleted,

                    // Remove espaços extra do nome da fase
                    CurrentPhase = p.CurrentPhase?.Trim() ?? "Concluído",

                    // ETA preenchido pelo Service depois de Task.WhenAll
                    EtaUtc = null,
                    EtaIsMlPrediction = false
                }).ToList()
            };
        }
    }
}