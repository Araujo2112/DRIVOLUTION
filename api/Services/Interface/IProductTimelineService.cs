using Drivolution.DTO;

namespace Drivolution.Services.Interface;

public interface IProductTimelineService
{
    Task<bool> ProductExists(int productId);
    Task<bool> ProductExistsBySerial(string serialNumber);

    Task<ProductTimelineResultDTO?> GetTimeline(int productId);
    Task<ProductTimelineResultDTO?> GetTimelineBySerial(string serialNumber);
}