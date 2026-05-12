using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;

namespace ApiTexPact.Services;

public class ProductPhaseService : IProductPhaseService
{
    private readonly IProductPhaseRepository _repo;

    public ProductPhaseService(IProductPhaseRepository repo)
    {
        _repo = repo;
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
    }

    // --- Auxiliar ---
    private static ProductPhaseDTO ToDTO(ProductPhaseModel pp) =>
        new(pp.Id, pp.ProductId, pp.ManufacturingPhaseId, pp.ManufacturingPhase?.Name ?? "",
            pp.WorkstationId, pp.Notes, pp.Result, pp.DatetimeIni, pp.DatetimeEnd, pp.QualityId);
}
