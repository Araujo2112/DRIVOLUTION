using ApiTexPact.Models;

namespace ApiTexPact.Services.RawMaterial.Interfaces.ItemInContainer
{
    public interface IItemInContainerService
    {
        Task<ItemInContainerModel> AddItemToContainerAsync(ItemInContainerModel item);
        
        Task<List<ItemInContainerModel>> GetAllItemsInContainerAsync();
        
        Task<ItemInContainerModel> GetItemByAsync(int itemInContainerId);
        Task<bool> RemoveItemFromContainerAsync(int itemInContainerId);

        
        Task<ItemInContainerModel> UpdateItemInContainerAsync(ItemInContainerModel item);
    }
}