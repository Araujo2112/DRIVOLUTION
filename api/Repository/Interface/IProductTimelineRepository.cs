using ApiTexPact.DTO;

namespace ApiTexPact.Repository.Interface;

public interface IProductTimelineRepository
{
    Task<bool> ProductExists(int productId);
    Task<List<ProductTimelineDTO>> GetTimeline(int productId);
}