using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using ApiTexPact.Services.Interface;
using ApiTexPact.Services.RawMaterial.Interfaces.ItemInContainer;

namespace ApiTexPact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemInContainerController : ControllerBase
    {
        private readonly IItemInContainerService _itemInContainerService;

        public ItemInContainerController(IItemInContainerService itemInContainerService)
        {
            _itemInContainerService = itemInContainerService ?? throw new ArgumentNullException(nameof(itemInContainerService));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItemsInContainerAsync()
        {
            try
            {
                var items = await _itemInContainerService.GetAllItemsInContainerAsync();
                if (items == null || !items.Any())
                {
                    return NotFound(new { message = "No items found." });
                }
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{itemInContainerId}")]
        public async Task<IActionResult> GetItemInContainerWithMaterialsAsync(int itemInContainerId)
        {
            try
            {
                var itemInContainer = await _itemInContainerService.GetItemByAsync(itemInContainerId);
                return Ok(itemInContainer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddItemToContainerAsync([FromBody] ItemInContainerDTO itemDto)
        {
            if (itemDto == null || itemDto.DateTimeIn >= itemDto.DateTimeOut)
            {
                return BadRequest(new { message = "Invalid parameters. Please check the provided data." });
            }

            try
            {
                var itemInContainer = new ItemInContainerModel
                {
                    ContainerId = itemDto.ContainerId,
                    DateTimeIn = itemDto.DateTimeIn,
                    DateTimeOut = itemDto.DateTimeOut
                };

                var createdItem = await _itemInContainerService.AddItemToContainerAsync(itemInContainer);

                var itemUrl = Url.Action("GetItemInContainer", "ItemInContainer", new { ItemInContainerId = createdItem.ItemInContainerId }, Request.Scheme);

                return Created(itemUrl, createdItem);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateItemInContainerAsync([FromBody] ItemInContainerDTO itemDto)
        {
            if (itemDto == null || itemDto.DateTimeIn >= itemDto.DateTimeOut)
            {
                return BadRequest(new { message = "Invalid parameters. Please check the provided data." });
            }

            try
            {
                var existingItem = await _itemInContainerService.GetItemByAsync(itemDto.itemInContainerId);

                if (existingItem == null)
                {
                    return NotFound(new { message = $"Item with ID {itemDto.itemInContainerId} not found." });
                }

                existingItem.DateTimeIn = itemDto.DateTimeIn;
                existingItem.DateTimeOut = itemDto.DateTimeOut;
                existingItem.ContainerId = itemDto.ContainerId;

                var updatedItem = await _itemInContainerService.UpdateItemInContainerAsync(existingItem);

                return Ok(new { message = $"Item {itemDto.itemInContainerId} updated successfully.", updatedItem });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpDelete("{itemInContainerId}")]
        public async Task<IActionResult> DeleteItemInContainerAsync(int itemInContainerId)
        {
            if (itemInContainerId == 0)
            {
                return BadRequest(new { message = "Item code cannot be empty." });
            }

            try
            {
                var deleted = await _itemInContainerService.RemoveItemFromContainerAsync(itemInContainerId);
                if (deleted)
                {
                    return NoContent();
                }

                return NotFound(new { message = $"Item with code {itemInContainerId} not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
