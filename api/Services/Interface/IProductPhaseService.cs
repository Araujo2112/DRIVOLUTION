using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IProductPhaseService
{
    /*
    Task → é um método assíncrono
    IEnumerable → devolve uma coleção (lista) de objetos
    ProductPhaseDTO → cada objeto da lista é um ProductPhaseDTO
    */
    Task<IEnumerable<ProductPhaseDTO>> GetByProduct(int productId);
    Task<ProductPhaseDTO?> GetCurrent(int productId); // ? -> pode devolver um ProductPhaseDTO ou null
    Task<ProductPhaseDTO> Create(CreateProductPhaseDTO dto); // Recebe CreateProductPhaseDTO com os dados da nova fase e depois devolce ProductPhaseDTO com a fase criada
    Task Close(int id, CloseProductPhaseDTO dto);
}
