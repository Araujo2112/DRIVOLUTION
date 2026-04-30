using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using ApiTexPact.DTO;
using ApiTexPact.Services.Interface.Containers;

namespace ApiTexPact.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContainerLocalizationHistoryController : ControllerBase
    {
        private readonly IContainerLocalizationService _service;

        public ContainerLocalizationHistoryController(IContainerLocalizationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContainerLocalizationsAsync()
        {
            var containerLocalizations = await _service.GetAllContainerLocalizationsAsync();
            return Ok(containerLocalizations); 
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContainerLocalizationById(int id)
        {
            try
            {
                var containerLocalizationHistory = await _service.GetContainerLocalizationByIdAsync(id);
                return Ok(containerLocalizationHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateContainerLocalizationHistory([FromBody] ContainerLocalizationDTO containerLocalizationCreateDTO)
        {
            try
            {
                if (containerLocalizationCreateDTO == null)
                {
                    return BadRequest("ContainerLocalizationCreateDTO object is null");
                }
                
                if (containerLocalizationCreateDTO.datetime == default)
                {
                    return BadRequest("The datetime field is required and must be a valid date.");
                }

           
                var containerLocalizationHistory = new ContainerLocalizationModel
                {
                    ContainerId = containerLocalizationCreateDTO.ContainerId,
                    SectionId = containerLocalizationCreateDTO.SectionId,
                    Datetime = containerLocalizationCreateDTO.datetime
                };

                var newContainerLocalizationHistory = await _service.CreateContainerLocalizationAsync(containerLocalizationHistory);

                return CreatedAtAction(
                    nameof(GetContainerLocalizationById), 
                    new { id = newContainerLocalizationHistory.ContainerId }, 
                    newContainerLocalizationHistory
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateContainerLocalizationHistory(
            [FromBody] ContainerLocalizationDTO body)
        {
            try
            {
                if (body == null)
                {
                    return BadRequest("Invalid data: DTO cannot be null.");
                }

                var updatedContainerLocalizationHistory = await _service.UpdateContainerLocalizationAsync(
                    body.Id,  
                    body.ContainerId,
                    body.SectionId,
                    body.datetime
                );

                return Ok(updatedContainerLocalizationHistory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContainerLocalizationHistory(int id)
        {
            try
            {
                var result = await _service.DeleteContainerLocalizationByIdAsync(id);
                if (!result)
                {
                    return NotFound($"ContainerLocalizationHistory with ID {id} not found.");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}