using ApiTexPact.DTO;
using ApiTexPact.Models;
using ApiTexPact.Services.Interface.Containers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ApiTexPact.Controllers.Container
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContainerController : ControllerBase
    {
        private readonly IContainerService _service;

        public ContainerController(IContainerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllContainersAsync()
        {
            var containers = await _service.GetAllContainersAsync();
            return Ok(containers);
        }

        [HttpGet("{containerId}")]
        public async Task<IActionResult> GetContainerByCode(int containerId)
        {
            try
            {
                var container = await _service.GetContainerId(containerId);
                return Ok(container);
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateContainer([FromBody] ContainerDTO containerDto)
        {
            try
            {
                var container = new ContainerModel
                {
                    ContainerCode = containerDto.ContainerCode,
                    ContainerName = containerDto.ContainerName,
                    ContainerVolume = containerDto.ContainerVolume,
                    Activate = containerDto.Activate
                };

                var newContainer = await _service.CreateContainerAsync(container);
                return CreatedAtAction(nameof(GetContainerByCode), new { containerId = newContainer.ContainerId }, newContainer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateContainer([FromBody] ContainerDTO containerDto)
        {
            try
            {
                var updatedContainer = await _service.UpdateContainerAsync(
                    containerDto.ContainerId,
                    containerDto.ContainerName,
                    containerDto.ContainerVolume,
                    containerDto.Activate
                );

                return Ok($"Container with code {containerDto.ContainerId} updated successfully.");
            }
            catch (Exception ex)
            {
                return NotFound(ex.Message);
            }
        }


        [HttpDelete("{containerId}")]
        public async Task<IActionResult> DeleteContainer(int containerId)
        {
            try
            {
                var success = await _service.DeleteContainerByCodeAsync(containerId);
                if (!success)
                {
                    return NotFound($"Container with code {containerId} not found.");
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
