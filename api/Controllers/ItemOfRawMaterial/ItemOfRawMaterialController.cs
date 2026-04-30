using ApiTexPact.Models;
using ApiTexPact.Service.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ApiTexPact.DTO;

namespace ApiTexPact.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemOfRawMaterialController : ControllerBase
{
    private readonly IItemOfRawMaterialService _service;

    public ItemOfRawMaterialController(IItemOfRawMaterialService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet]
    public async Task<IActionResult> GetAllItemsAsync()
    {
        try
        {
            var items = await _service.GetAllAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemOfRawMaterialModel>> GetById(int id)
    {
        var item = await _service.GetByCodeAsync(id); 
        if (item == null)
            return NotFound();

        return Ok(item);
    }


    [HttpPost]
    public async Task<IActionResult> CreateItemAsync([FromBody] ItemOfRawMaterialDTO itemDto)
    {
        try
        {
            if (itemDto == null)
                return BadRequest("Request body is null.");

            var createdItem = await _service.CreateAsync(itemDto);
            return Created($"api/ItemOfRawMaterial/{createdItem.ItemRawId}", createdItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{itemRawId}")]
    public async Task<IActionResult> UpdateItemAsync(int itemRawId, [FromBody] ItemOfRawMaterialDTO itemDto)
    {
        try
        {
            if (itemDto == null)
                return BadRequest("Request body is null.");

            itemDto.ItemRawId = itemRawId; // Ensure path ID is used
            var updatedItem = await _service.UpdateAsync(itemDto);
            return Ok(updatedItem);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpDelete("{itemRawId}")]
    public async Task<IActionResult> DeleteItemAsync(int itemRawId)
    {
        try
        {
            var success = await _service.DeleteByCodeAsync(itemRawId);
            if (!success)
                return NotFound($"Item of raw material with code {itemRawId} not found.");

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
