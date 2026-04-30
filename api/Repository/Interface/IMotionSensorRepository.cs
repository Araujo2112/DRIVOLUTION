using ApiTexPact.Models;

namespace ApiTexPact.Repository.Interface;

public interface IMotionSensorRepository
{
    Task<MotionSensorModel> GetByPorticoId(string porticoId);
    Task Update(MotionSensorModel motionSensor);
}