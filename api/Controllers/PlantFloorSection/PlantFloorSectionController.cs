using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ApiTexPact.DTO;
using ApiTexPact.Services.PlantFloorSection;

namespace ApiTexPact.Controllers.PlantFloorSection
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantFloorSectionController : ControllerBase
    {
        private readonly IPlantFloorSectionService _service;

        public PlantFloorSectionController(IPlantFloorSectionService repository)
        {
            _service = repository ?? throw new ArgumentNullException(nameof(repository));
        }

   
        [HttpGet]
        public async Task<IActionResult> GetAllSectionsAsync()
        {
            var sections = await _service.GetAllSectionsAsync();
            return Ok(sections);
        }

    
        [HttpGet("{sectionId}")]
        public async Task<IActionResult> GetSectionByCode(int sectionId)
        {
            try
            {
             
                var section = await _service.GetSectionById(sectionId);
                if (section == null)
                {
                    return NotFound($"Section with code {sectionId} not found.");
                }

               
                var responseDto = new PlantFloorSectionDTO
                {
                    SectionId = section.SectionId,
                    SectionCode = section.SectionCode,
                    Name = section.name,
                };

        
                return Ok(responseDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateSection([FromBody] PlantFloorSectionDTO sectionDto)
        {
            try
            {
                var section = new PlantFloorSectionModel
                {
                    SectionId = sectionDto.SectionId,
                    SectionCode = sectionDto.SectionCode,
                    name = sectionDto.Name
                };

                var newSection = await _service.CreateSectionAsync(section);
                
                return CreatedAtAction(nameof(GetSectionByCode), new { sectionId = newSection.SectionId }, newSection);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


    
        [HttpPut]
        public async Task<IActionResult> UpdateSection([FromBody] PlantFloorSectionDTO sectionDto)
        {
            try
            {
                var updatedSection = await _service.UpdateSectionAsync(sectionDto.SectionId, sectionDto.Name);
                if (updatedSection == null)
                {
                    return NotFound($"Section with code {sectionDto.SectionId} not found.");
                }
                return Ok($"Section with code {sectionDto.SectionId} updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

      
        [HttpDelete("{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            try
            {
                var result = await _service.DeleteSectionByCodeAsync(sectionId);
                if (!result)
                {
                    return NotFound($"Section with code {sectionId} not found.");
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