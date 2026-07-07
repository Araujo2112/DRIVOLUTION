using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

public class ProductPhaseService : IProductPhaseService
{
    private readonly IProductPhaseRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IManufacturingOrderRepository _moRepo;
    private readonly INotificationService _notificationService;
    private readonly IQualityCheckRepository _qualityRepo;

    public ProductPhaseService(
        IProductPhaseRepository repo,
        IProductRepository productRepo,
        IManufacturingOrderRepository moRepo,
        INotificationService notificationService,
        IQualityCheckRepository qualityRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
        _moRepo = moRepo;
        _notificationService = notificationService;
        _qualityRepo = qualityRepo;
    }

    public async Task<IEnumerable<ProductPhaseDTO>> GetByProduct(int productId)
    {
        var items = await _repo.GetByProduct(productId);
        return items.Select(ToDTO);
    }

    public async Task<ProductPhaseDTO?> GetCurrent(int productId)
    {
        var item = await _repo.GetCurrentByProduct(productId);
        return item == null ? null : ToDTO(item);
    }

    public async Task<ProductPhaseDTO> Create(CreateProductPhaseDTO dto)
{
    var openPhases = await _repo.GetAllOpenByProduct(dto.ProductId);

    foreach (var open in openPhases)
    {
        if (open.ManufacturingPhaseId == dto.ManufacturingPhaseId)
        {
            throw new InvalidOperationException(
                "Este produto já está nesta fase."
            );
        }

        var hasPassedQuality = await _qualityRepo.HasPassedForProductPhaseAsync(
            dto.ProductId,
            open.ManufacturingPhaseId
        );

        if (!hasPassedQuality)
        {
            throw new InvalidOperationException(
                $"Produto bloqueado: a fase anterior ({open.ManufacturingPhase?.Name ?? open.ManufacturingPhaseId.ToString()}) não tem QualityCheck com estado 'passed'."
            );
        }
    }

    foreach (var open in openPhases)
    {
        open.DatetimeEnd = DateTime.UtcNow;
        await _repo.Update(open);
    }

    var entity = new ProductPhaseModel
    {
        ProductId = dto.ProductId,
        ManufacturingPhaseId = dto.ManufacturingPhaseId,
        WorkstationId = dto.WorkstationId,
        Notes = dto.Notes,
        DatetimeIni = DateTime.UtcNow
    };

    var created = await _repo.Create(entity);

    await UpdateMoStatus(dto.ProductId);

    return ToDTO(created);
}

    public async Task Close(int id, CloseProductPhaseDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) throw new KeyNotFoundException("Fase não encontrada.");

        entity.DatetimeEnd = DateTime.UtcNow;
        entity.Result = dto.Result;
        entity.QualityId = dto.QualityId;

        await _repo.Update(entity);

        await UpdateMoStatus(entity.ProductId);
    }

    private async Task UpdateMoStatus(int productId)
    {
        var product = await _productRepo.GetById(productId);
        if (product == null) return;

        var mo = await _moRepo.GetByIdWithDetails(product.ManufacturingOrderId);
        if (mo == null) return;

        if (mo.Status == OrderStatus.Completed || mo.Status == OrderStatus.Cancelled) return;

        var wasPending = mo.Status == OrderStatus.Pending;
        var allProducts = mo.Products.ToList();

        bool anyInProgress = allProducts.Any(p =>
            p.ProductPhases.Any(pp => pp.DatetimeEnd == null));

        bool allCompleted = allProducts.All(p =>
            p.ProductPhases.Any() &&
            p.ProductPhases.All(pp => pp.DatetimeEnd != null));

        if (allCompleted)
        {
            mo.Status = OrderStatus.Completed;
            mo.EndDate = DateTime.UtcNow;
        }
        else if (anyInProgress)
        {
            mo.Status = OrderStatus.InProgress;
        }

        await _moRepo.Update(mo);

        var appUserId = mo.ClientOrder?.AppUserId;
        if (appUserId == null) return;

        if (wasPending && mo.Status == OrderStatus.InProgress)
        {
            await _notificationService.CreateAsync(
                appUserId.Value,
                "order_started",
                $"A tua encomenda {mo.ClientOrder!.OrderNumber} foi iniciada.",
                clientOrderId: mo.ClientOrderId);
        }

        var thisProduct = allProducts.FirstOrDefault(p => p.Id == productId);
        if (thisProduct != null &&
            thisProduct.ProductPhases.Any() &&
            thisProduct.ProductPhases.All(pp => pp.DatetimeEnd != null))
        {
            await _notificationService.CreateAsync(
                appUserId.Value,
                "car_completed",
                $"O carro {thisProduct.SerialNumber ?? "#" + thisProduct.Id} da encomenda {mo.ClientOrder!.OrderNumber} foi concluído.",
                clientOrderId: mo.ClientOrderId,
                productId: thisProduct.Id);
        }

        if (mo.Status == OrderStatus.Completed)
        {
            var siblingMos = await _moRepo.GetByClientOrderId(mo.ClientOrderId);
            if (siblingMos.All(m => m.Status == OrderStatus.Completed))
            {
                var already = await _notificationService.ExistsAsync("order_completed", mo.ClientOrderId);
                if (!already)
                {
                    await _notificationService.CreateAsync(
                        appUserId.Value,
                        "order_completed",
                        $"A tua encomenda {mo.ClientOrder!.OrderNumber} foi concluída.",
                        clientOrderId: mo.ClientOrderId);
                }
            }
        }
    }

    private static ProductPhaseDTO ToDTO(ProductPhaseModel pp) =>
        new(pp.Id, pp.ProductId, pp.ManufacturingPhaseId, pp.ManufacturingPhase?.Name ?? "",
            pp.WorkstationId, pp.Notes, pp.Result, pp.DatetimeIni, pp.DatetimeEnd, pp.QualityId);
}