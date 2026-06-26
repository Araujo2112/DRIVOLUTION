using Drivolution.DTO;

namespace Drivolution.Repository.Interface;

public interface IProductTimelineRepository
{
    Task<bool> ProductExists(int productId);
    Task<bool> ProductExistsBySerial(string serialNumber);

    Task<List<ProductTimelineDTO>> GetTimeline(int productId);
    Task<List<ProductTimelineDTO>> GetTimelineBySerial(string serialNumber);
}