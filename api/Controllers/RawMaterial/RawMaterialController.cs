using ApiTexPact.Models;
using ApiTexPact.Repository;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ApiTexPact.DTO;
using ApiTexPact.Repository.Interface;
using ApiTexPact.Service.Interface;

namespace ApiTexPact.Controllers.RawMaterial
{
    [Route("api/[controller]")]
    [ApiController]
    public class RawMaterialController : ControllerBase
    {
        private readonly IRawMaterialService _service;

        public RawMaterialController(IRawMaterialService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRawMaterialsAsync()
        {
            try
            {
                var rawMaterials = await _service.GetAllRawMaterialsAsync();
                return Ok(rawMaterials);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRawMaterialByIdAsync(int id)
        {
            try
            {
                var rawMaterial = await _service.GetRawMaterialByIdAsync(id);
                return Ok(rawMaterial);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateRawMaterial([FromBody] RawMaterialDTO rawMaterialDto)
        {
            try
            {
                var rawMaterial = new RawMaterialModel
                {
                    Name = rawMaterialDto.Name,
                    Info = rawMaterialDto.Info
                };

                var newRawMaterial = await _service.CreateRawMaterialAsync(rawMaterial);

            
                return Created($"api/RawMaterial/{newRawMaterial.RawId}", newRawMaterial);
                
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); 
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateRawMaterialAsync([FromBody] RawMaterialDTO rawMaterialDto)
        {
            try
            {
                if (rawMaterialDto.Id <= 0)
                {
                    return BadRequest("Invalid RawMaterial ID.");
                }
                
                var rawMaterial = new RawMaterialModel
                {
                    RawId = rawMaterialDto.Id,  
                    Name = rawMaterialDto.Name,
                    Info = rawMaterialDto.Info
                };
                
                var updatedRawMaterial = await _service.UpdateRawMaterialAsync(rawMaterialDto.Id, rawMaterial);
                if (updatedRawMaterial == null)
                {
                    return NotFound($"RawMaterial with ID {rawMaterialDto.Id} not found.");
                }

                return Ok(updatedRawMaterial);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRawMaterialAsync(int id)
        {
            try
            {
                var result = await _service.DeleteRawMaterialByIdAsync(id);
                if (!result)
                {
                    return NotFound($"RawMaterial with ID {id} not found.");
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
