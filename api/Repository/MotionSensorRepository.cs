using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class MotionSensorRepository : IMotionSensorRepository
{
    private readonly ApplicationDbContext _context;

    public MotionSensorRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<MotionSensorModel> GetByPorticoId(string porticoId)
    {
        return await _context.MotionSensors
            .FirstOrDefaultAsync(ms => ms.PorticoId == porticoId);
    }

    public async Task Update(MotionSensorModel motionSensor)
    {
        _context.MotionSensors.Update(motionSensor);
        await _context.SaveChangesAsync();
    }
}