using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ApiTexPact.DTO;
using System.Linq;
using System.Text;
using ApiTexPact.Services;
using Newtonsoft.Json;

namespace ApiTexPact.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LotOfRawMaterialController : ControllerBase
{
    private readonly ILotOfRawMaterialService _service;

    public LotOfRawMaterialController(ILotOfRawMaterialService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpGet]
    public async Task<IActionResult> GetAlllotsAsync()
    {
        try
        {
            var lots = await _service.GetAllLotsAsync();
            return Ok(lots);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpGet("{lotId}")]
    public async Task<IActionResult> GetLotByCodeAsync(int lotId)
    {
        try
        {
            var lot = await _service.GetLotByCodeAsync(lotId);

            if (lot == null)
            {
                return NotFound($"Lot with code {lotId} not found.");
            }

            return Ok(lot);  
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }



    [HttpPost]
    public async Task<IActionResult> CreateLotAsync([FromBody] LotOfRawMaterialDTO lotDTO)
    {
        try
        {
            var existingLot = await _service.GetLotByCodeAsync(lotDTO.LotId);
            if (existingLot != null)
            {
                return Conflict($"Lot with code {lotDTO.LotId} already exists.");
            }

           
            var lot = new LotOfRawMaterialModel
            {
                LotId = lotDTO.LotId,
                LotCode = lotDTO.LotCode,
                LotNumber = lotDTO.LotNumber,
                LotQuantity = lotDTO.LotQuantity,
                LotUnit = lotDTO.LotUnit,
                RawMaterialId = lotDTO.RawMaterialId 
            };

       
            var newLot = await _service.CreateLotAsync(lot);

      
            return Created($"api/LotOfRawMaterial/{newLot.LotId}", newLot);
            
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }


    [HttpPut]
    public async Task<IActionResult> UpdatelotAsync([FromBody] LotOfRawMaterialDTO lotDTO)
    {
        try
        {
            var existingLot = await _service.GetLotByCodeAsync(lotDTO.LotId);
            if (existingLot == null)
            {
                return NotFound($"Lot with code {lotDTO.LotId} not found.");
            }
            
            var result = await _service.UpdateLotAsync(
                lotDTO.LotId, 
                lotDTO.LotNumber, 
                lotDTO.LotCode,
                lotDTO.LotQuantity, 
                lotDTO.LotUnit
            );

            if (result == null)
            {
                return NotFound($"Lot with code {lotDTO.LotId} not found.");
            }

            return Ok($"Lot with code {lotDTO.LotId} updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }





    [HttpDelete("{lotId}")]
    public async Task<IActionResult> DeletelotAsync(int lotId)
    {
        try
        {
            var result = await _service.DeleteLotByCodeAsync(lotId);
            if (!result)
            {
                return NotFound($"lot with code {lotId} not found.");
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
