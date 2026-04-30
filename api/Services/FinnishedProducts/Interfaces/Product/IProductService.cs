using ApiTexPact.DTO;

namespace ApiTexPact.Services.Interface.Product;

public interface IProductService
{
    Task<IEnumerable<ProductDTO>> GetAllProducts();
    Task<ProductDTO> GetProductById(int id);
    Task<ProductDTO> CreateProduct(CreateProductDTO productDto);
    Task<ProductDTO> UpdateProduct(int id, UpdateProductDTO productDto);
    Task DeleteProduct(int id);
}