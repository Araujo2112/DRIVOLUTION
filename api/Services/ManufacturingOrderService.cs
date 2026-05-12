using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Models.Constants;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Services.Interface;

namespace ApiTexPact.Services;

public class ManufacturingOrderService : IManufacturingOrderService
{
    private readonly IManufacturingOrderRepository _repo;

    public ManufacturingOrderService(IManufacturingOrderRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<ManufacturingOrderDTO>> GetAll()
    {
        var items = await _repo.GetAll();
        return items.Select(MapToDTO);
    }

    public async Task<ManufacturingOrderDTO?> GetById(int id)
    {
        var item = await _repo.GetById(id);
        return item == null ? null : MapToDTO(item);
    }

    public async Task<object?> GetByIdWithDetails(int id) => await _repo.GetByIdWithDetails(id);

    public async Task<IEnumerable<ManufacturingOrderDTO>> GetByStatus(string status)
    {
        var items = await _repo.GetByStatus(status);
        return items.Select(MapToDTO);
    }

    public async Task<ManufacturingOrderDTO> Create(CreateManufacturingOrderDTO dto)
    {
        var entity = new ManufacturingOrderModel
        {
            ClientOrderId = dto.ClientOrderId,
            ManufacturingOrderNumber = dto.ManufacturingOrderNumber,
            StartDate = dto.StartDate,
            Status = EntityStatus.Pending // USA A CONSTANTE
        };

        var created = await _repo.Create(entity);
        return MapToDTO(created);
    }

    public async Task<bool> Update(int id, UpdateManufacturingOrderDTO dto)
    {
        var entity = await _repo.GetById(id);
        if (entity == null) return false;

        if (dto.Status != null) entity.Status = dto.Status;
        if (dto.EndDate != null) entity.EndDate = dto.EndDate;

        await _repo.Update(entity);
        return true;
    }

    public async Task<bool> Delete(int id)
    {
        if (!await _repo.Exists(id)) return false;
        await _repo.Delete(id);
        return true;
    }

    // Helper para não repetir código de mapeamento
    private ManufacturingOrderDTO MapToDTO(ManufacturingOrderModel mo) =>
        new ManufacturingOrderDTO(
            mo.Id, 
            mo.ClientOrderId, 
            mo.ClientOrder?.CustomerName ?? "", 
            mo.ManufacturingOrderNumber, 
            mo.StartDate, 
            mo.EndDate, 
            mo.Status
        );
}