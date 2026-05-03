using System.Data;
using ApiTexPact.Data;
using ApiTexPact.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiTexPact.Controllers;

[ApiController]
[Route("api/products")]
public class ProductExecutionController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly OrionService _orion;

    public ProductExecutionController(ApplicationDbContext context, OrionService orion)
    {
        _context = context;
        _orion = orion;
    }

    public class AdvanceProductRequest
    {
        public int WorkstationId { get; set; }
        public string? Notes { get; set; }
    }

    [HttpPost("{productId}/advance")]
    public async Task<IActionResult> AdvanceProduct(int productId, [FromBody] AdvanceProductRequest request)
    {
        if (request.WorkstationId <= 0)
            return BadRequest("A valid workstationId is required.");

        var connection = _context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            var modelId = await GetIntAsync(connection, transaction,
                "SELECT model_id FROM product WHERE id = @productId",
                ("@productId", productId));

            if (modelId == null)
                return NotFound("Product does not exist.");

            var workstationExists = await GetIntAsync(connection, transaction,
                "SELECT id FROM workstation WHERE id = @workstationId",
                ("@workstationId", request.WorkstationId));

            if (workstationExists == null)
                return BadRequest("Workstation does not exist.");

            var flowExists = await GetIntAsync(connection, transaction,
                """
                SELECT id
                FROM phase_sequence
                WHERE model_id = @modelId
                LIMIT 1
                """,
                ("@modelId", modelId.Value));

            if (flowExists == null)
                return BadRequest("This product model has no production flow defined.");

            var activePhaseId = await GetIntAsync(connection, transaction,
                """
                SELECT id
                FROM product_phase
                WHERE product_id = @productId
                  AND datetime_end IS NULL
                ORDER BY datetime_ini DESC
                LIMIT 1
                """,
                ("@productId", productId));

            if (activePhaseId != null)
            {
                await ExecuteAsync(connection, transaction,
                    """
                    UPDATE product_phase
                    SET datetime_end = NOW(),
                        result = 'completed'
                    WHERE id = @activePhaseId
                    """,
                    ("@activePhaseId", activePhaseId.Value));
            }

            var nextPhase = await GetNextPhaseAsync(connection, transaction, productId, modelId.Value);

            if (nextPhase == null)
            {
                await transaction.CommitAsync();

                await _orion.UpsertProductAsync(
                    productId,
                    "completed",
                    request.WorkstationId,
                    "completed"
                );

                return Ok(new
                {
                    productId,
                    status = "completed",
                    message = "Product has completed all manufacturing phases."
                });
            }

            var newProductPhaseId = await InsertProductPhaseAsync(
                connection,
                transaction,
                productId,
                nextPhase.Value.ManufacturingPhaseId,
                request.WorkstationId,
                request.Notes
            );

            await transaction.CommitAsync();

            await _orion.UpsertProductAsync(
                productId,
                nextPhase.Value.PhaseName,
                request.WorkstationId,
                "in_progress"
            );

            return Ok(new
            {
                productId,
                status = "advanced",
                productPhaseId = newProductPhaseId,
                currentPhase = new
                {
                    manufacturingPhaseId = nextPhase.Value.ManufacturingPhaseId,
                    phaseName = nextPhase.Value.PhaseName,
                    order = nextPhase.Value.Order,
                    workstationId = request.WorkstationId
                }
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, ex.Message);
        }
    }

    private static async Task<int?> GetIntAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        foreach (var parameter in parameters)
        {
            var p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.Value = parameter.Value;
            command.Parameters.Add(p);
        }

        var result = await command.ExecuteScalarAsync();

        if (result == null || result == DBNull.Value)
            return null;

        return Convert.ToInt32(result);
    }

    private static async Task ExecuteAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        string sql,
        params (string Name, object Value)[] parameters)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;

        foreach (var parameter in parameters)
        {
            var p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.Value = parameter.Value;
            command.Parameters.Add(p);
        }

        await command.ExecuteNonQueryAsync();
    }

    private static async Task<(int ManufacturingPhaseId, string PhaseName, int Order)?> GetNextPhaseAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        int productId,
        int modelId)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            SELECT ps.manufacturing_phase_id, mp.name, ps."order"
            FROM phase_sequence ps
            JOIN manufacturing_phase mp ON mp.id = ps.manufacturing_phase_id
            WHERE ps.model_id = @modelId
              AND ps."order" > COALESCE((
                    SELECT MAX(ps2."order")
                    FROM product_phase pp
                    JOIN phase_sequence ps2
                      ON ps2.manufacturing_phase_id = pp.manufacturing_phase_id
                     AND ps2.model_id = @modelId
                    WHERE pp.product_id = @productId
                ), 0)
            ORDER BY ps."order"
            LIMIT 1
            """;

        var p1 = command.CreateParameter();
        p1.ParameterName = "@modelId";
        p1.Value = modelId;
        command.Parameters.Add(p1);

        var p2 = command.CreateParameter();
        p2.ParameterName = "@productId";
        p2.Value = productId;
        command.Parameters.Add(p2);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return (
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetInt32(2)
        );
    }

    private static async Task<int> InsertProductPhaseAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction transaction,
        int productId,
        int manufacturingPhaseId,
        int workstationId,
        string? notes)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO product_phase
                (notes, result, condition, datetime_ini, datetime_end,
                 manufacturing_phase_id, product_id, workstation_id, quality_id)
            VALUES
                (@notes, 'in_progress', 'normal', NOW(), NULL,
                 @manufacturingPhaseId, @productId, @workstationId, NULL)
            RETURNING id
            """;

        var parameters = new (string Name, object? Value)[]
        {
            ("@notes", string.IsNullOrWhiteSpace(notes) ? DBNull.Value : notes),
            ("@manufacturingPhaseId", manufacturingPhaseId),
            ("@productId", productId),
            ("@workstationId", workstationId)
        };

        foreach (var parameter in parameters)
        {
            var p = command.CreateParameter();
            p.ParameterName = parameter.Name;
            p.Value = parameter.Value ?? DBNull.Value;
            command.Parameters.Add(p);
        }

        var result = await command.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }
}