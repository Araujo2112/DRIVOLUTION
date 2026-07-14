using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Models.Constants;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Implementa o contrato definido em IManufacturingOrderService
public class ManufacturingOrderService : IManufacturingOrderService
{
    // Repository responsável pelas ordens de fabrico
    private readonly IManufacturingOrderRepository _repo;

    // O ASP.NET injeta automaticamente o repository
    public ManufacturingOrderService(IManufacturingOrderRepository repo)
    {
        _repo = repo;
    }

    // Devolve uma lista paginada de ordens de fabrico
    public async Task<PagedResultDTO<ManufacturingOrderDTO>> GetPaged(
        int page,
        int pageSize,
        string? search,
        string? status,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        // Vai buscar os dados ao repository
        var paged = await _repo.GetPaged(
            page,
            pageSize,
            search,
            status,
            dateFrom,
            dateTo
        );

        // Converte as entidades para DTO e mantém a informação da paginação
        return new PagedResultDTO<ManufacturingOrderDTO>
        {
            Data = paged.Data.Select(MapToDTO),
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    // Procura uma ordem de fabrico pelo ID
    public async Task<ManufacturingOrderDTO?> GetById(int id)
    {
        // Procura a ordem
        var item = await _repo.GetById(id);

        // Se existir converte para DTO; caso contrário devolve null
        return item == null ? null : MapToDTO(item);
    }

    // Procura uma ordem com toda a informação relacionada
    public async Task<ManufacturingOrderDetailDTO?> GetByIdWithDetails(int id)
    {
        // Obtém a ordem com produtos, configurações e fases
        var mo = await _repo.GetByIdWithDetails(id);

        // Se não existir devolve null
        if (mo == null) return null;

        // Constrói um DTO mais completo com toda a informação
        return new ManufacturingOrderDetailDTO(
            mo.Id,
            mo.ClientOrderId,

            // Nome do cliente
            mo.ClientOrder?.AppUser?.Name ?? "",

            mo.ManufacturingOrderNumber,
            mo.StartDate,
            mo.EndDate,
            mo.Status,

            // Converte todos os produtos da ordem
            mo.Products.Select(p => new ProductDetailDTO(

                p.Id,
                p.SerialNumber,
                p.LotNumber,
                p.CarModel?.Name,
                p.ProductionDate,

                // Converte todas as configurações do produto
                p.ProductConfigs.Select(pc =>
                    new ProductConfigDetailDTO(
                        pc.ConfigOptionId,
                        pc.ConfigOption?.Config?.Item ?? "",
                        pc.ConfigOption?.Value ?? ""
                    )
                ).ToList(),

                // Converte todas as fases do produto
                p.ProductPhases.Select(pp =>
                    new ProductPhaseDetailDTO(
                        pp.Id,
                        pp.ManufacturingPhase?.Name,
                        pp.DatetimeIni,
                        pp.DatetimeEnd,
                        pp.Result,
                        pp.Notes
                    )
                ).ToList()

            )).ToList()
        );
    }

    // Cria uma nova ordem de fabrico
    public async Task<ManufacturingOrderDTO> Create(
        CreateManufacturingOrderDTO dto)
    {
        // Cria a entidade que será guardada na base de dados
        var entity = new ManufacturingOrderModel
        {
            ClientOrderId = dto.ClientOrderId,
            ManufacturingOrderNumber =
                dto.ManufacturingOrderNumber,

            StartDate = dto.StartDate,

            // Todas as ordens começam com o estado Pending
            Status = OrderStatus.Pending
        };

        // Guarda a entidade na base de dados
        var created = await _repo.Create(entity);

        // Converte a entidade criada para DTO
        return MapToDTO(created);
    }

    // Atualiza uma ordem existente
    public async Task<bool> Update(
        int id,
        UpdateManufacturingOrderDTO dto)
    {
        // Procura a ordem
        var entity = await _repo.GetById(id);

        // Se não existir devolve false
        if (entity == null)
            return false;

        // Só atualiza os campos enviados
        if (dto.Status != null)
            entity.Status = dto.Status;

        if (dto.EndDate != null)
            entity.EndDate = dto.EndDate;

        // Guarda as alterações
        await _repo.Update(entity);

        return true;
    }

    // Elimina uma ordem de fabrico
    public async Task<bool> Delete(int id)
    {
        // Verifica primeiro se a ordem existe
        if (!await _repo.Exists(id))
            return false;

        // Remove a ordem da base de dados
        await _repo.Delete(id);

        return true;
    }

    // Método auxiliar usado para converter
    // ManufacturingOrderModel em ManufacturingOrderDTO
    private ManufacturingOrderDTO MapToDTO(
        ManufacturingOrderModel mo) =>
        new ManufacturingOrderDTO(
            mo.Id,
            mo.ClientOrderId,

            // Nome do cliente associado à ordem
            mo.ClientOrder?.AppUser?.Name ?? "",

            mo.ManufacturingOrderNumber,
            mo.StartDate,
            mo.EndDate,
            mo.Status
        );
}