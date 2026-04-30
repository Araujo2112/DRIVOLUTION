using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface
{
    public interface IItemInContainerRepository
    {
        Task<ItemInContainerModel> AddItemToContainerAsync(ItemInContainerModel item);

        Task<List<ItemInContainerModel>> GetAllItemInContainerAsync();

        Task<ItemInContainerModel> GetItemAsync(int itemInContainerId);

        Task<bool> RemoveItemFromContainerAsync(int itemInContainerId);

        Task<ItemInContainerModel> UpdateItemInContainerAsync(ItemInContainerModel item);
    }
}