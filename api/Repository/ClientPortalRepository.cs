using Drivolution.Data;
using Drivolution.DTO;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository
{
    public class ClientPortalRepository : IClientPortalRepository
    {
        private readonly ApplicationDbContext _context;

        public ClientPortalRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<ClientOrderSummaryDTO>> GetOrdersByClientAsync(int appUserId)
        {
            // Encomendas do cliente com contagem de carros concluídos
            // Critério de "concluído": produto tem product_phase da fase "Inspeção" com datetime_end IS NOT NULL.
            // Trim() porque manufacturing_phase.name é VARCHAR sem normalização garantida na BD.
            var raw = await _context.ClientOrders
                .Where(co => co.AppUserId == appUserId)
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
                            ModelName = p.CarModel.Name,
                            IsCompleted = p.ProductPhases
                                .Any(pp => pp.ManufacturingPhase.Name.Contains("Inspeção")
                                           && pp.DatetimeEnd != null)
                        })
                        .ToList()
                })
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var orders = raw.Select(o => new ClientOrderSummaryDTO
            {
                Id = o.Id,
                OrderNumber = o.OrderNumber,
                OrderDate = o.OrderDate,
                TotalCars = o.Products.Count,
                CompletedCars = o.Products.Count(p => p.IsCompleted),
                // Uma encomenda pode ter produtos de modelos diferentes (agrega vários
                // ManufacturingOrders); mostramos os modelos distintos em vez de assumir 1:1.
                ModelName = string.Join(" / ", o.Products.Select(p => p.ModelName).Distinct()),
                PendingProductIds = o.Products.Where(p => !p.IsCompleted).Select(p => p.Id).ToList()
            }).ToList();

            // Preenche o campo Status com texto legível
            foreach (var o in orders)
                o.Status = $"{o.CompletedCars} de {o.TotalCars} concluídos";

            return orders;
        }

        public async Task<ClientOrderDetailDTO?> GetOrderDetailAsync(int orderId, int appUserId)
        {
            var order = await _context.ClientOrders
                .Where(co => co.Id == orderId && co.AppUserId == appUserId)
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
                            ModelName = p.CarModel.Name,
                            IsCompleted = p.ProductPhases
                                .Any(pp => pp.ManufacturingPhase.Name.Contains("Inspeção")
                                           && pp.DatetimeEnd != null),
                            CurrentPhase = p.ProductPhases
                                .Where(pp => pp.DatetimeEnd == null)
                                .OrderByDescending(pp => pp.DatetimeIni)
                                .Select(pp => pp.ManufacturingPhase.Name)
                                .FirstOrDefault() ?? "Concluído"
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null) return null;

            return new ClientOrderDetailDTO
            {
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                OrderDate = order.OrderDate,
                Products = order.Products.Select(p => new ClientProductDTO
                {
                    ProductId = p.Id,
                    SerialNumber = p.SerialNumber,
                    ModelName = p.ModelName,
                    IsCompleted = p.IsCompleted,
                    CurrentPhase = p.CurrentPhase?.Trim() ?? "Concluído",
                    // ETA preenchido pelo Service depois de Task.WhenAll
                    EtaUtc = null,
                    EtaIsMlPrediction = false
                }).ToList()
            };
        }
    }
}