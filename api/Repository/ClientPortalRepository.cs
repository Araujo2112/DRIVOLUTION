using Drivolution.Data;
using Drivolution.DTO.Client;
using Drivolution.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace Drivolution.Repository;

public class ClientPortalRepository : IClientPortalRepository
{
    private readonly ApplicationDbContext _context;

    public ClientPortalRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ClientOrderSummaryDTO>> GetOrders(int appUserId)
    {
        return await _context.ClientOrders
            .Where(co => co.AppUserId == appUserId)
            .Select(co => new ClientOrderSummaryDTO
            {
                Id = co.Id,
                OrderNumber = co.OrderNumber,
                OrderDate = co.OrderDate,
                Quantity = co.Quantity,

                CompletedProducts = co.ManufacturingOrders
                    .SelectMany(mo => mo.Products)
                    .Count(p =>
                        p.ProductPhases.Any() &&
                        p.ProductPhases.All(pp => pp.DatetimeEnd != null)
                    ),

                Status = co.ManufacturingOrders
                    .SelectMany(mo => mo.Products)
                    .Any(p => p.ProductPhases.Any(pp => pp.DatetimeEnd == null))
                        ? "in_progress"
                        : co.ManufacturingOrders.SelectMany(mo => mo.Products).Any()
                            ? "completed"
                            : "pending"
            })
            .OrderByDescending(co => co.OrderDate)
            .ToListAsync();
    }

    public async Task<List<ClientOrderProductDTO>> GetProducts(int appUserId, int orderId)
    {
        var orderBelongsToClient = await _context.ClientOrders
            .AnyAsync(co => co.Id == orderId && co.AppUserId == appUserId);

        if (!orderBelongsToClient)
            return new List<ClientOrderProductDTO>();

        return await _context.Products
            .Where(p =>
                p.ManufacturingOrder.ClientOrderId == orderId &&
                p.ManufacturingOrder.ClientOrder.AppUserId == appUserId
            )
            .Select(p => new ClientOrderProductDTO
            {
                Vin = p.SerialNumber ?? string.Empty,

                CurrentPhase = p.ProductPhases
                    .OrderByDescending(pp => pp.DatetimeIni)
                    .Select(pp => pp.ManufacturingPhase.Name)
                    .FirstOrDefault() ?? "Pendente",

                EstimatedFinish = p.ProductPhases
                    .OrderByDescending(pp => pp.DatetimeIni)
                    .Select(pp =>
                        pp.DatetimeEnd != null
                            ? pp.DatetimeEnd
                            : (DateTime?)pp.DatetimeIni.AddMinutes(
                                pp.ManufacturingPhase.EstimatedDuration ?? 0
                            )
                    )
                    .FirstOrDefault()
            })
            .OrderBy(p => p.Vin)
            .ToListAsync();
    }
}