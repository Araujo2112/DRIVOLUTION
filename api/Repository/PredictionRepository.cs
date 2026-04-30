using ApiTexPact.Data;
using ApiTexPact.Models;
using ApiTexPact.Repository.Interface.Prediction;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Repository;

public class PredictionRepository : IPredictionRepository
{
    private readonly ApplicationDbContext _context;

    public PredictionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PredictionModel> CreateAsync(PredictionModel model)
    {
        _context.Prediction.Add(model);
        await _context.SaveChangesAsync();
        return model;
    }

    public async Task<PredictionModel> GetByIdAsync(int id)
    {
        return await _context.Prediction.FindAsync(id);
    }

    public async Task<List<PredictionModel>> GetAllAsync()
    {
        return await _context.Prediction.ToListAsync();
    }

    public async Task<PredictionModel> UpdateAsync(PredictionModel model)
    {
        _context.Prediction.Update(model);
        await _context.SaveChangesAsync();
        return model;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var model = await _context.Prediction.FindAsync(id);
        if (model == null)
            return false;

        _context.Prediction.Remove(model);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Dictionary<string, object>>> GetTrainingDatasetAsync(DateTime cutoffUtc)
    {
        var raw = await (
            from ph in _context.ManufacturingOrderPhases
            join mp in _context.ManufacturingPhases on ph.ManufacturingPhaseId equals mp.Id
            join mo in _context.ManufacturingOrders on ph.ManufacturingOrderId equals mo.Id
            join pl in _context.ProductLots on mo.ProductLotId equals pl.Id
            where ph.DateTimeEnd > ph.DateTimeInit
                  && ph.DateTimeEnd > cutoffUtc
            select new
            {
                ph.Quantity,
                ph.ManufacturingPhaseId,
                mp.PlantFloorSectionId,
                mo.ProductLotId,
                mo.ClientId,
                pl.LotQuantity,
                ph.SheduleInit,
                Seconds = EF.Functions.DateDiffSecond(ph.DateTimeInit, ph.DateTimeEnd)
            }).ToListAsync();

        return raw.Select(r => new Dictionary<string, object>
        {
            ["Quantity"] = r.Quantity,
            ["PhaseId"] = r.ManufacturingPhaseId,
            ["SectionId"] = r.PlantFloorSectionId,
            ["ProductId"] = r.ProductLotId,
            ["ClientId"] = r.ClientId,
            ["LotQty"] = r.LotQuantity,
            ["Hour"] = r.SheduleInit.Hour,
            ["WeekDay"] = (int)r.SheduleInit.DayOfWeek,
            ["label"] = r.Seconds
        }).ToList();
    }

    public async Task<PredictionModel> GetLatestModelAsync()
    {
        return await _context.Prediction
            .OrderByDescending(p => p.Id)
            .FirstOrDefaultAsync();
    }
}