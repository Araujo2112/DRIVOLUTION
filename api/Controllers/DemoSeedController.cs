using ApiTexPact.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/demo")]
public class DemoSeedController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DemoSeedController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedDemoData()
    {
        var suffix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var carModelId = 1;

        var clientOrderId = await InsertInt($"""
            INSERT INTO client_order (order_number, order_date, customer_name, quantity)
            VALUES ('CO-{suffix}', NOW(), 'Cliente Teste', 1)
            RETURNING id
        """);

        var manufacturingOrderId = await InsertInt($"""
            INSERT INTO manufacturing_order (client_order_id, manufacturing_order_number, start_date)
            VALUES ({clientOrderId}, 'MO-{suffix}', NOW())
            RETURNING id
        """);

        var productId = await InsertInt($"""
            INSERT INTO product (manufacturing_order_id, model_id, serial_number, lot_number)
            VALUES ({manufacturingOrderId}, {carModelId}, 'VIN-{suffix}', 'LOT-{suffix}')
            RETURNING id
        """);

        var productionLineId = await InsertInt("""
            INSERT INTO production_line (name, location, status, capacity)
            VALUES ('Main Assembly Line', 'Factory A', 'active', 100)
            RETURNING id
        """);

        var workstationId = await InsertInt($"""
            INSERT INTO workstation (production_line_id, type)
            VALUES ({productionLineId}, 'Assembly Station')
            RETURNING id
        """);

        var manufacturingPhaseId = await InsertInt("""
            INSERT INTO manufacturing_phase (name, estimated_duration)
            VALUES ('Packaging', 60)
            RETURNING id
        """);

        var productPhaseId = await InsertInt($"""
            INSERT INTO product_phase
                (product_id, manufacturing_phase_id, workstation_id, result, notes, datetime_ini, datetime_end)
            VALUES
                ({productId}, {manufacturingPhaseId}, {workstationId}, 'in_progress', 'Started Packaging phase', NOW(), NULL)
            RETURNING id
        """);

        return Ok(new
        {
            message = "Demo data created successfully.",
            carModelId,
            clientOrderId,
            manufacturingOrderId,
            productId,
            productionLineId,
            workstationId,
            manufacturingPhaseId,
            productPhaseId
        });
    }

    private async Task<int> InsertInt(string sql)
    {
        await using var command = _context.Database.GetDbConnection().CreateCommand();

        if (command.Connection!.State != System.Data.ConnectionState.Open)
            await command.Connection.OpenAsync();

        command.CommandText = sql;

        var result = await command.ExecuteScalarAsync();

        return Convert.ToInt32(result);
    }
}