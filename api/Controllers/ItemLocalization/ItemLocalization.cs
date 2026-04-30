using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApiTexPact.Service.Interface;

namespace ApiTexPact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemLocalization : ControllerBase
    {
        private readonly IItemLocalizationService _service;

        public ItemLocalization(IItemLocalizationService service)
        {
            _service = service;
        }

       
        [HttpGet("{itemLocalizationId}")]
        public async Task<IActionResult> GetItemLocalizationByIdAsync(int itemLocalizationId)
        {
            try
            {
                var itemLocalization = await _service.GetItemLocalizationByIdAsync(itemLocalizationId);
                if (itemLocalization == null)
                {
                    return NotFound($"ItemLocalization with ID {itemLocalizationId} not found.");
                }

                return Ok(itemLocalization);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

      
        [HttpGet]
        public async Task<IActionResult> GetAllItemLocalizationsAsync()
        {
            try
            {
                var itemLocalizations = await _service.GetAllItemLocalizationsAsync();
                if (itemLocalizations == null || itemLocalizations.Count == 0)
                {
                    return NotFound("No item localizations found.");
                }

                return Ok(itemLocalizations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving ItemLocalizations: {ex.Message}");
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateItemLocalization([FromBody] ItemLocalizationDTO itemDto)
        {
            try
            {
                var itemLocalization = new ItemLocalizationModel
                {
                    ItemRawId = itemDto.ItemRawId,
                    ContainerLocalizationId = itemDto.ContainerLocalizationId,
                    DateTime = itemDto.DateTime
                };

                var newItemLocalization = await _service.CreateItemLocalizationAsync(itemLocalization);
                return Created($"api/ItemLocalization/{newItemLocalization.ItemLocalizationId}", newItemLocalization);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error creating ItemLocalization: {ex.Message}");
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateItemLocalization([FromBody] ItemLocalizationDTO itemLocalization)
        {
            if (itemLocalization == null)
            {
                return BadRequest("Dados inválidos.");
            }

            try
            {
                var existingItemLocalization = await _service.GetItemLocalizationByIdAsync(itemLocalization.ItemRawId);
                if (existingItemLocalization == null)
                {
                    return NotFound($"ItemLocalization com ID {itemLocalization.ItemRawId} não encontrado.");
                }
                
                var updatedItemLocalization = await _service.UpdateItemLocalizationAsync(itemLocalization.ItemRawId, itemLocalization.DateTime);

                if (updatedItemLocalization == null)
                {
                    return NotFound("Falha ao atualizar a localização do item.");
                }

                return Ok(updatedItemLocalization);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno no servidor: {ex.Message}");
            }
        }


        [HttpDelete("{itemLocalizationId}")]
        public async Task<IActionResult> DeleteItemLocalization(int itemLocalizationId)
        {
            try
            {
                var success = await _service.DeleteItemLocalizationAsync(itemLocalizationId);
                if (!success)
                {
                    return NotFound($"ItemLocalization with ID {itemLocalizationId} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error deleting ItemLocalization: {ex.Message}");
            }
        }
    }
}
