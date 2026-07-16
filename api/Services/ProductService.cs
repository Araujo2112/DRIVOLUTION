using Drivolution.DTO;
using Drivolution.Models;
using Drivolution.Repository.Interface;
using Drivolution.Services.Interface;

namespace Drivolution.Services;

// Service responsável pela lógica de negócio dos produtos
public class ProductService : IProductService
{
    // Repository responsável pelo acesso aos produtos
    private readonly IProductRepository _repo;

    // O ASP.NET injeta automaticamente o repository
    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    // Devolve uma lista paginada de produtos, podendo aplicar filtros
    public async Task<PagedResultDTO<ProductDTO>> GetPaged(
        int page,
        int pageSize,
        string? search,
        int? modelId,
        DateTime? dateFrom,
        DateTime? dateTo)
    {
        // Obtém os produtos filtrados através do repository
        var paged = await _repo.GetPaged(
            page,
            pageSize,
            search,
            modelId,
            dateFrom,
            dateTo
        );

        // Converte cada produto para DTO antes de devolver
        return new PagedResultDTO<ProductDTO>
        {
            Data = paged.Data.Select(MapToDTO),
            Total = paged.Total,
            Page = paged.Page,
            PageSize = paged.PageSize
        };
    }

    // Procura um produto pelo seu ID
    public async Task<ProductDTO?> GetById(int id)
    {
        // Obtém o produto
        var item = await _repo.GetById(id);

        // Se existir converte para DTO, caso contrário devolve null
        return item == null ? null : MapToDTO(item);
    }

    // Devolve todos os produtos pertencentes a uma ordem de fabrico
    public async Task<IEnumerable<ProductDTO>> GetByManufacturingOrder(int orderId)
    {
        // Obtém os produtos da ordem de fabrico
        var items = await _repo.GetByManufacturingOrder(orderId);

        // Converte todos os produtos para DTO
        return items.Select(MapToDTO);
    }

    // Cria um novo produto
    public async Task<ProductDTO> Create(CreateProductDTO dto)
    {
        // Cria a entidade ProductModel a partir dos dados recebidos
        var entity = new ProductModel
        {
            ManufacturingOrderId = dto.ManufacturingOrderId,
            ModelId = dto.ModelId,
            SerialNumber = dto.SerialNumber,
            LotNumber = dto.LotNumber,
        };

        // Guarda o produto na base de dados
        var created = await _repo.Create(entity);

        // Devolve o produto criado em formato DTO
        return MapToDTO(created);
    }

    // Atualiza um produto existente
    public async Task<bool> Update(int id, UpdateProductDTO dto)
    {
        // Procura o produto
        var entity = await _repo.GetById(id);

        // Se não existir devolve false
        if (entity == null)
            return false;

        // Atualiza apenas os campos enviados
        if (dto.ProductionDate != null)
            entity.ProductionDate = dto.ProductionDate;

        // Guarda as alterações
        await _repo.Update(entity);

        return true;
    }

    // Elimina um produto
    public async Task<bool> Delete(int id)
    {
        // Verifica se o produto existe
        if (!await _repo.Exists(id))
            return false;

        // Elimina o produto
        await _repo.Delete(id);

        return true;
    }

    // Método auxiliar que converte ProductModel para ProductDTO
    private static ProductDTO MapToDTO(ProductModel p) =>
        new ProductDTO(
            p.Id,
            p.ManufacturingOrderId,
            p.ModelId,
            p.CarModel?.Name,
            p.SerialNumber,
            p.LotNumber,
            p.ProductionDate
        );
}