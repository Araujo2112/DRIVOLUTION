using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Models.Constants;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;

namespace ApiTexPact.Services;

public class ProductPhaseService : IProductPhaseService
{
    private readonly IProductPhaseRepository _repo;
    private readonly IProductRepository _productRepo;
    private readonly IManufacturingOrderRepository _moRepo;

    public ProductPhaseService(
        IProductPhaseRepository repo,
        IProductRepository productRepo,
        IManufacturingOrderRepository moRepo)
    {
        _repo = repo;
        _productRepo = productRepo;
        _moRepo = moRepo;
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
        // Fechar a fase atual antes de abrir uma nova
        var current = await _repo.GetCurrentByProduct(dto.ProductId);
        if (current != null)
        {
            current.DatetimeEnd = DateTime.UtcNow;
            await _repo.Update(current);
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

        // ── Atualizar status da MO para InProgress ────────────────────────────
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

        // ── Verificar se todos os produtos da MO estão concluídos ─────────────
        await UpdateMoStatus(entity.ProductId);
    }

    // ── Lógica de atualização automática do status da MO ─────────────────────
    private async Task UpdateMoStatus(int productId)
    {
        var product = await _productRepo.GetById(productId);
        if (product == null) return;

        var mo = await _moRepo.GetByIdWithDetails(product.ManufacturingOrderId);
        if (mo == null) return;

        // Se já está concluída ou cancelada, não tocar
        if (mo.Status == EntityStatus.Completed || mo.Status == EntityStatus.Cancelled) return;

        var allProducts = mo.Products.ToList();

        // Verificar se todos os produtos têm pelo menos uma fase aberta (sem DatetimeEnd)
        bool anyInProgress = allProducts.Any(p =>
            p.ProductPhases.Any(pp => pp.DatetimeEnd == null));

        // Verificar se todos os produtos têm fases e todas fechadas
        bool allCompleted = allProducts.All(p =>
            p.ProductPhases.Any() &&
            p.ProductPhases.All(pp => pp.DatetimeEnd != null));

        if (allCompleted)
        {
            mo.Status = EntityStatus.Completed;
            mo.EndDate = DateTime.UtcNow;
        }
        else if (anyInProgress)
        {
            mo.Status = EntityStatus.InProgress;
        }

        await _moRepo.Update(mo);
    }

    private static ProductPhaseDTO ToDTO(ProductPhaseModel pp) =>
        new(pp.Id, pp.ProductId, pp.ManufacturingPhaseId, pp.ManufacturingPhase?.Name ?? "",
            pp.WorkstationId, pp.Notes, pp.Result, pp.DatetimeIni, pp.DatetimeEnd, pp.QualityId);
}